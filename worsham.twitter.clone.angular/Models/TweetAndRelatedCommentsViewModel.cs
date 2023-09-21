using worsham.twitter.clone.angular.Models.EntityModels;

namespace worsham.twitter.clone.angular.Models
{
    public class TweetAndRelatedCommentsViewModel
    {
        public Tweets? Tweet { get; set; }
        public string? TweetOwnerName { get; set; }
        public string? TweetOwnersProfilePicture { get; set; }
        public List<Comments>? Comments { get; set; }
    }
}
