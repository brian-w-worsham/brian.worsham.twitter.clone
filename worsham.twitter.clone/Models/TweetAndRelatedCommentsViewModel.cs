using worsham.twitter.clone.Models.EntityModels;

namespace worsham.twitter.clone.Models
{
    public class TweetAndRelatedCommentsViewModel
    {
        public Tweets? Tweet { get; set; }
        public string? TweetOwnerName { get; set; }
        public string? TweetOwnersProfilePicture { get; set; }
        public List<Comments>? Comments { get; set; }
    }
}
