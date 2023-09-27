import { Tweets } from './tweets';
import { Users } from './users';

export class ReTweets {
  constructor(
    public id: number,
    public originalTweetId: number,
    public reTweetCreationDateTime: string,
    public retweeterId: number,
    public originalTweet: Tweets,
    public retweeter: Users
  ) {}
}
