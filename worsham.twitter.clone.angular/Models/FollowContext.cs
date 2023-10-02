namespace worsham.twitter.clone.angular.Models
{
    public class FollowContext
    {
        public int? FollowId { get; set; }
        public bool CurrentUserIsFollowing { get; set; }
        public int? followedUserId { get; set; }
    }
}
