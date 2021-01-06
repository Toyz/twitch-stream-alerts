using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Website.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class TwitchController : ControllerBase
    {
        private readonly ILogger<TwitchController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string TwitchSignatureKey;

        public TwitchController(ILogger<TwitchController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            TwitchSignatureKey = _configuration.GetValue("twitch:signature_key", "");
        }

        [HttpGet]
        public IActionResult Get()
        {
            var scheme = Request.Scheme;
            if(!Request.Host.Host.Contains("localhost"))
            {
                scheme = "https";
            }

            return Ok(new
            {
                url = Url.Action("Notify", "Twitch", new { }, scheme),
            });
        }

        [HttpPost("notify")]
        public async Task<IActionResult> Notify()
        {
            (string sig, string id, string ts) Headers;
            if(!ValidateHeaders(HttpContext.Request.Headers, out Headers))
            {
                return BadRequest();
            }

            string body = await new StreamReader(Request.Body).ReadToEndAsync();
            if (!VerifyHash(Headers.sig, Headers.id, Headers.ts, body))
            {
                return BadRequest();
            }

            if(!HttpContext.Request.Headers.TryGetValue("Twitch-Eventsub-Message-Type", out var payloadType))
            {
                return BadRequest();
            }

            switch(payloadType)
            {
                case "webhook_callback_verification":
                    var doc = JsonSerializer.Deserialize<JsonDocument>(body);
                    if(doc.RootElement.TryGetProperty("code", out var code))
                    {
                        var strCode = code.GetString();
                        _logger.LogInformation(strCode);

                        HttpContext.Response.ContentType = "text/plain";
                        return Content(strCode);
                    }

                    return BadRequest();
                case "notification":
                    var payload = JsonSerializer.Deserialize<Models.TwitchEvent>(body);
                    _logger.LogInformation(body);
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

            return localHash == sig.ToUpper();
        }

        private byte[] HashHMAC(byte[] key, byte[] message)
        {
            var hash = new HMACSHA256(key);
            return hash.ComputeHash(message);
        }
    }
}
