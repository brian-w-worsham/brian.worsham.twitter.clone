using worsham.twitter.clone.angular.Models.EntityModels;

namespace worsham.twitter.clone.angular.Models
{
    public class LikedTweetInfo
    {
        public Tweets LikedTweet { get; set; }
        public int LikedTweetId { get; set; }
        public string OriginalUserName { get; set; }
        public string OriginalProfilePictureUrl { get; set; }
        public DateTime OriginalTweetCreationDateTime { get; set; }
        public string OriginalTweetContent { get; set; }
    }
}
