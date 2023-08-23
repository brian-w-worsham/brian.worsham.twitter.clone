using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using worsham.twitter.clone.Utils;

namespace worsham.twitter.clone.Models
{
    public class ProfileModel
    {
        [Required]
        [StringLength(20)]
        [Unicode(false)]
        public string UserName { get; set; }
        [StringLength(160)]
        [Unicode(false)] 
        public string Bio { get; set; }
        [StringLength(300)]
        [Unicode(false)]
        public string ProfilePictureUrl { get; set; }
        public int FollowingCount { get; set; }
        public int FollowersCount { get; set; }
        public int TweetsCount { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsCurrentUser { get; set; }
        public bool HasErrors { get; set; }
        public IEnumerable<string> ValidationErrors { get; set; }
        public IEnumerable<TweetModel> Tweets { get; set; } 
    }
}
