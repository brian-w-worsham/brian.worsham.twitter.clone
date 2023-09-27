import { Likes } from './likes';
import { Comments } from './comments';
import { ReTweets } from './reTweets';

export class TweetModel {
  constructor(
    public id: number,
    public timeSincePosted: string,
    public timeAgo: string,
    public content: string,
    public tweeterUserId: number,
    public tweeterUserName: string,
    public tweeterProfilePictureUrl: string,
    public likes: Likes[] | null,
    public comments: Comments[] | null,
    public retweets: ReTweets[] | null
  ) {}
}
