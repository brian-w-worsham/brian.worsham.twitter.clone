import { Comments } from './comments';
import { Likes } from './likes';
import { ReTweets } from './reTweets';
import { Users } from './users';

export class Tweets {
  constructor(
    public id: number,
    public content: string,
    public creationDateTime: string,
    public tweeterId: number,
    public tweeter: Users,
    public comments: Comments[] | null,
    public likes: Likes[] | null,
    public retweets: ReTweets[] | null
  ) {
    this.comments = comments || [];
    this.likes = likes || [];
    this.retweets = retweets || [];
  }
}
