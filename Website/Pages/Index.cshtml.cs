using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Website.Models;
using Website.Repos;

namespace Website.Pages
{
    public class IndexModel : PageModel
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
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
        {

        }
    }
}
