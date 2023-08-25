using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using worsham.twitter.clone.Utils;
using worsham.twitter.clone.Models.EntityModels;

namespace worsham.twitter.clone.Models
{
    public class ProfileModel
    {
        public Users User { get; set; }
        public int TweetsCount { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsCurrentUser { get; set; }
        public bool HasErrors { get; set; }
        public IEnumerable<string> ValidationErrors { get; set; }
        public IEnumerable<TweetModel> Tweets { get; set; } 
    }
}
