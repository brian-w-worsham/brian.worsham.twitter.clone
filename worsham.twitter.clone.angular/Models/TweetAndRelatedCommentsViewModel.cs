using worsham.twitter.clone.angular.Models.EntityModels;

namespace worsham.twitter.clone.angular.Models
{
    public class TweetAndRelatedCommentsViewModel
    {
        // public Tweets? Tweet { get; set; }
        public string? TweetOwnerName { get; set; }
        public string? TweetOwnersProfilePicture { get; set; }

        public string? TweetContent { get; set; }
        public DateTime TweetCreationDateTime { get; set; }
        public List<CommentModelView>? TweetComments { get; set; }
    }

    public class CommentModelView
    {
        public int CommentId { get; set; }
        public int CommenterId { get; set; }
        public string? CommenterUserName { get; set; }
        public string? CommentersProfilePicture { get; set; }
        public string? CommentContent { get; set; }
    }
}
