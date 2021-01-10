using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Website.Models;
using Website.Repos;

namespace Website.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        public class RegisterModel
        {
            [JsonPropertyName("streamer")]
            [Required]
            public string Streamer { get; set; }

            [JsonPropertyName("discord_url")]
            [Required]
            public string DiscordURL { get; set; }
        }

        private readonly IAlertRepo _alertRepo;
        private readonly UserManager<User> _userManager;

        public UserController(IAlertRepo alertRepo, UserManager<User> userManager)
        {
            _alertRepo = alertRepo;
            _userManager = userManager;
        }

        [HttpPost("register_webhook")]
        public async Task<IActionResult> Register([FromBody]RegisterModel registerModel)
        {
            if (registerModel == null || string.IsNullOrEmpty(registerModel.Streamer) || string.IsNullOrEmpty(registerModel.DiscordURL)) return BadRequest();

            var user = await _userManager.GetUserAsync(HttpContext.User); 

            return BadRequest();
        }
    }
}
