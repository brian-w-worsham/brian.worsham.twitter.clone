import { Likes } from './likes';
import { Comments } from './comments';
import { ReTweets } from './reTweets';

export class TweetModel {
  constructor(
    public Id: number,
    public TimeSincePosted: string,
    public TimeAgo: string,
    public Content: string,
    public TweeterUserId: number,
    public TweeterUserName: string,
    public TweeterProfilePictureUrl: string,
    public Likes: Likes[] | null,
    public Comments: Comments[] | null,
    public Retweets: ReTweets[] | null
  ) {}
}
