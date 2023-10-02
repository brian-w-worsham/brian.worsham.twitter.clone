using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using worsham.twitter.clone.angular.Models.EntityModels;

namespace worsham.twitter.clone.angular.Models
{
    public class UserProfileModel
    {
        public int UserId { get; set; }
        [Required]
        [StringLength(20)]
        [Unicode(false)]
        public string UserName { get; set; }
        [StringLength(160)]
        [Unicode(false)]
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public IFormFile? FormFile { get; set; }
        public int? FollowersCount { get; set; }
        public int? FollowingCount { get; set; }
        public bool? UserIsViewingOwnProfile { get; set; }
        public bool? CurrentUserIsFollowing { get; set; }
        public int? FollowId { get; set; }
        public string? ErrorNotification { get; set; }
        public List<ProfilePictureUrlModel>? TweeterProfilePictureUrls { get; set; }
        public List<ProfilePictureUrlModel>? RetweeterProfilePictureUrls { get; set; }
        public List<ProfilePictureUrlModel>? LikedProfilePictureUrls { get; set; }
        public List<TweetModel>? Tweets { get; set; }
        public List<TweetModel>? LikedTweets { get; set; }
        public List<TweetModel>? ReTweets { get; set; }
        public List<RetweetedTweetInfo>? RetweetedTweets { get; set; }
        public List<LikedTweetInfo>? LikedTweetInfos { get; set; }
    }
}
