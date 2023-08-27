using worsham.twitter.clone.Models.EntityModels;

namespace worsham.twitter.clone.Models
{
    public class RetweetedTweetInfo
    {
        public Tweets OriginalTweet { get; set; }
        public string OriginalUserName { get; set; }
        public DateTime OriginalTweetCreationDateTime { get; set; }
        public string OriginalTweetContent { get; set; }
    }
}
