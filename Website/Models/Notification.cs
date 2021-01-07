using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class Notification
    {
        // This is the twitch webhook ID
        [Key]
        public string Id { get; set; }

        [Required]
        public string Owner { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public List<string> TriggerUrls { get; set; }

        public Notification() { }

        public Notification(string owner)
        {
            Owner = owner;
            Created = DateTime.UtcNow;
            TriggerUrls = new List<string>();
        }

        public Notification(string owner, List<string> urls)
        {
            Owner = owner;
            Created = DateTime.UtcNow;
            TriggerUrls = urls;
        } 
    }
}
