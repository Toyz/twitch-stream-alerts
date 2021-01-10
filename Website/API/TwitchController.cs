using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Website.Models;

namespace Website.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class TwitchController : ControllerBase
    {
        private readonly ILogger<TwitchController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string TwitchSignatureKey;
        private readonly string NotificationURL;
        private readonly Repos.IStreamerRepo _streamerRepo;

        public TwitchController(ILogger<TwitchController> logger, IConfiguration configuration, Repos.IStreamerRepo streamerRepo)
        {
            _logger = logger;
            _configuration = configuration;

            TwitchSignatureKey = _configuration.GetValue("twitch:signature_key", string.Empty);
            NotificationURL = _configuration.GetValue("notify_host", string.Empty);
            _streamerRepo = streamerRepo;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var scheme = Request.Scheme;
            if(!Request.Host.Host.Contains("localhost"))
            {
                scheme = "https";
            }
            var pageUrl = Url.Action("Notify", "Twitch", new { }, scheme);

            if (!string.IsNullOrEmpty(NotificationURL))
            {
                pageUrl = $"{NotificationURL.TrimEnd('/')}/api/twitch/notify";
            }

            return Ok(new
            {
                url = pageUrl
            });
        }

#if DEBUG
        [HttpGet("list")]
        [Authorize]
        public async Task<IActionResult> List()
        {
            var clientId = _configuration.GetValue("twitch:ClientID", "");
            var clientSec = _configuration.GetValue("twitch:ClientSecret", "");
            var client = "";
            using (var wc = new WebClient())
            {
                var data = await wc.UploadStringTaskAsync($"https://id.twitch.tv/oauth2/token?client_id={clientId}&client_secret={clientSec}&grant_type=client_credentials", "POST", string.Empty);

                _logger.LogInformation(data);
                var json = JsonSerializer.Deserialize<JsonDocument>(data);

                client = json.RootElement.GetProperty("access_token").GetString();
            }

            using (var wc = new HttpClient())
            {
                wc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", client);
                wc.DefaultRequestHeaders.Add("Client-ID", clientId);

                var data = await wc.GetAsync("https://api.twitch.tv/helix/eventsub/subscriptions");

                return Ok(await data.Content.ReadAsStringAsync());
            }
        }
#endif

        /*
        [HttpPost("register")]
        [Authorize]
        public async Task<IActionResult> Register([FromBody])
        {
            if (string.IsNullOrEmpty(boardcasterid)) return BadRequest();

            var clientId = _configuration.GetValue("twitch:ClientID", "");
            var clientSec = _configuration.GetValue("twitch:ClientSecret", "");
            var client = "";
            using(var wc = new WebClient())
            {
                var data = await wc.UploadStringTaskAsync($"https://id.twitch.tv/oauth2/token?client_id={clientId}&client_secret={clientSec}&grant_type=client_credentials", "POST", string.Empty);

                _logger.LogInformation(data);
                var json = JsonSerializer.Deserialize<JsonDocument>(data);

                client = json.RootElement.GetProperty("access_token").GetString();       
            }

            var scheme = Request.Scheme;
            if (!Request.Host.Host.Contains("localhost"))
            {
                scheme = "https";
            }
            var pageUrl = Url.Action("Notify", "Twitch", new { }, scheme);

            if (!string.IsNullOrEmpty(NotificationURL))
            {
                pageUrl = $"{NotificationURL.TrimEnd('/')}/api/twitch/notify";
            }

            using (var wc = new HttpClient())
            {
                wc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", client);
                wc.DefaultRequestHeaders.Add("Client-ID", clientId);

                var subEvent = new TwitchSubscribe();
                subEvent.Type = "stream.online";
                subEvent.Version = "1";
                subEvent.Condition = new Condition()
                {
                    BroadcasterUserId = boardcasterid
                };
                subEvent.Transport = new Transport()
                {
                    Method = "webhook",
                    Callback = pageUrl,
                    Secret = _configuration.GetValue("twitch:signature_key", "")
                };

                try
                {
                    var content = new StringContent(subEvent.ToString(), Encoding.UTF8, "application/json");

                    var data = await wc.PostAsync("https://api.twitch.tv/helix/eventsub/subscriptions", content);
                    var body = await data.Content.ReadAsStringAsync();

                    return Ok(body);
                }catch(WebException ex)
                {
                    using (Stream stream = ex.Response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        _logger.LogError(reader.ReadToEnd());
                    }
                } 
            }

            return BadRequest();
        }
        */

        [HttpPost("notify")]
        public async Task<IActionResult> Notify()
        {
            string body = await new StreamReader(Request.Body).ReadToEndAsync();
            _logger.LogInformation(body);

            (string sig, string id, string ts) Headers;
            if(!ValidateHeaders(HttpContext.Request.Headers, out Headers))
            {
                return BadRequest();
            }

            if (!VerifyHash(Headers.sig, Headers.id, Headers.ts, body))
            {
                return BadRequest();
            }

            if(!HttpContext.Request.Headers.TryGetValue("Twitch-Eventsub-Message-Type", out var payloadType))
            {
                return BadRequest();
            }

            var payload = JsonSerializer.Deserialize<TwitchEvent>(body);

            switch (payloadType)
            {
                case "webhook_callback_verification":
                    if(!string.IsNullOrEmpty(payload.Challenge))
                    {
                        _logger.LogInformation(payload.Challenge);

                        HttpContext.Response.ContentType = "text/plain";

                        var streamer = await _streamerRepo.GetById(payload.Subscription.Condition.BroadcasterUserId);
                        streamer.Verified = true;
                        await _streamerRepo.Update(streamer);

                        return Content(payload.Challenge);
                    }

                    return BadRequest();
                case "notification":
                    var payloadData = payload.EventObject;

                    switch(payloadData)
                    {
                    }

                    return Ok(payload.Subscription.Id);

                default:
                   return BadRequest(); 
            }
        }

        private bool ValidateHeaders(IHeaderDictionary headers, out (string sig, string id, string ts) headers2)
        {
            headers2 = ("", "", "");

            if (!headers.TryGetValue("Twitch-Eventsub-Message-Signature", out var messageSig)) return false;
            if (!headers.TryGetValue("Twitch-Eventsub-Message-Id", out var messageId)) return false;
            if (!headers.TryGetValue("Twitch-Eventsub-Message-Timestamp", out var messageTimeStamp)) return false;

            headers2 = (messageSig.ToString().Split("=").Last(), messageId, messageTimeStamp);
            return true;
        }

        private bool VerifyHash(string sig, string id, string ts, string body)
        {
            var payload = id + ts + body;
            var data = HashHMAC(Encoding.UTF8.GetBytes(TwitchSignatureKey), Encoding.UTF8.GetBytes(payload));

            var localHash = BitConverter.ToString(data).Replace("-", "");

            return localHash.Equals(sig, StringComparison.OrdinalIgnoreCase);
        }

        private byte[] HashHMAC(byte[] key, byte[] message)
        {
            var hash = new HMACSHA256(key);
            return hash.ComputeHash(message);
        }
    }
}
