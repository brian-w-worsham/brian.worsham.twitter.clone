namespace worsham.twitter.clone.angular.Models
{
    public class CreateCommentModalModel
    {
        public int TweetId { get; set; }
        public int Index { get; set; }
        public int? CurrentUserId { get; set; }
        public string? TweeterUserName { get; set; }
        public string? TimeAgo { get; set; }
        public string? TweetContent { get; set; }
    }
}
