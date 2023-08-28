using worsham.twitter.clone.Models.EntityModels;

namespace worsham.twitter.clone.Models
{
    public class UserProfileModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public IFormFile? FormFile { get; set; }
        public int? FollowersCount { get; set; }
        public int? FollowingCount { get; set; }
        public List<Tweets>? Tweets { get; set; }
        public List<Tweets>? LikedTweets { get; set; }
        public List<Tweets>? ReTweets { get; set; }
        public List<RetweetedTweetInfo>? RetweetedTweets { get; set; }
        public List<LikedTweetInfo>? LikedTweetInfos { get; set; }
    }
}
