using worsham.twitter.clone.angular.Models.EntityModels;

namespace worsham.twitter.clone.angular.Models
{
    public class RetweetedTweetInfo
    {
        public Tweets OriginalTweet { get; set; }
        public string OriginalUserName { get; set; }
        public DateTime OriginalTweetCreationDateTime { get; set; }
        public string OriginalTweetContent { get; set; }
    }
}
