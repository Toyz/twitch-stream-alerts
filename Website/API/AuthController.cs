using AspNet.Security.OAuth.Twitch;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Website.Models;

namespace Website.API
{
    [Route("Auth")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _config;

        public AuthController(UserManager<User> userManager, ILogger<AuthController> logger, IConfiguration config)
        {
            _userManager = userManager;
            _logger = logger;
            _config = config;
        }

        [AllowAnonymous]
        [HttpGet("Login")]
        public async Task Login(string returnUrl = "/")
        {
            var scheme = Request.Scheme;
            if (!Request.Host.Host.Contains("localhost"))
            {
                scheme = "https";
            }

            await HttpContext.ChallengeAsync(TwitchAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = new PathString("/Auth/ExternalAuthLogin").Add(QueryString.Create("returnUrl", returnUrl)),
            });
        }

        [HttpGet("Logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return Redirect("/");
        }


        [AllowAnonymous]
        [HttpGet("ExternalAuthLogin")]
        public async Task<ActionResult> ExternalAuthLogin(string returnUrl = "/")
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            var cur = HttpContext.User.Claims;

            var user = new User
            {
                AccessToken = cur.First(x => x.Type == "urn:twitch:access_token").Value,
                RefreshToken = cur.First(x => x.Type == "urn:twitch:refresh_token").Value,
                ExpireTime = DateTime.Parse(cur.First(x => x.Type == "urn:twitch:expires").Value),
                Id = cur.First(x => x.Type == ClaimTypes.NameIdentifier).Value,
                Email = cur.First(x => x.Type == ClaimTypes.Email).Value,
                UserName = cur.First(x => x.Type == TwitchAuthenticationConstants.Claims.DisplayName).Value,
                Avatar = cur.First(x => x.Type == TwitchAuthenticationConstants.Claims.ProfileImageUrl).Value
            };

            var currentUser = await _userManager.FindByIdAsync(user.Id);
            if (currentUser != null)
            {
                currentUser.LockoutEnabled = false;
                currentUser.AccessToken = user.AccessToken;
                currentUser.RefreshToken = user.RefreshToken;
                currentUser.ExpireTime = user.ExpireTime;

                await _userManager.UpdateAsync(currentUser);
            }
            else
            {
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                await _userManager.CreateAsync(user);
            }

            await HttpContext.SignOutAsync();

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim("avatar", user.Avatar));

            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return Redirect(returnUrl);
        }
    }
}
