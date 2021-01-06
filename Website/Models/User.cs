using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class User : IdentityUser
    {
        public DateTime ExpireTime { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string Avatar { get; set; }
    }
}
