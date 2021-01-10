using System;
using System.ComponentModel.DataAnnotations;

namespace Website.Models
{
    public class Streamer
    {
        [Key]
        public string StreamId { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public bool Verified { get; set; }

        public Streamer() { }

        public Streamer(string streamId)
        {
            StreamId = streamId;
            Created = DateTime.UtcNow;
            Verified = false;
        }
    }
}
