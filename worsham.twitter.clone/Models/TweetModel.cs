using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using worsham.twitter.clone.Models.EntityModels;

namespace worsham.twitter.clone.Models
{
    public class TweetModel
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Posted")]
        [DataType(DataType.Duration)]
        public TimeSpan TimeSincePosted { get; set; }
        
        [Display(Name = "Tweet")]
        [Required]
        [StringLength(280, MinimumLength = 1, ErrorMessage = "Tweet must be between 1 and 280 characters.")]
        [Unicode(false)]
        public string? Content { get; set; }

        public int TweeterUserId { get; set; }

        [Required]
        [StringLength(20)]
        [Unicode(false)]
        [Display(Name = "Tweeter")]
        public string? TweeterUserName { get; set; }

        public List<Likes>? Likes { get; set; }
        public List<Comments>? Comments { get; set; }
        public List<ReTweets>? Retweets { get; set; }
    }
}
