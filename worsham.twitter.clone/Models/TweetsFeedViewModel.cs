using Microsoft.AspNetCore.Mvc.Rendering;

namespace worsham.twitter.clone.Models
{
    public record TweetsFeedViewModel
    (
        //ProfileModel UserProfile,
        bool HasErrors,
        IEnumerable<string> ValidationErrors,
        IEnumerable<TweetModel> Tweets,
        PostModel Post
    );
}
