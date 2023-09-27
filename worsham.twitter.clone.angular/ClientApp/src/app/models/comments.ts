import { Tweets } from './tweets';
import { Users } from './users';

export class Comments {
  constructor(
    public id: number,
    public content: string,
    public originalTweetId: number,
    public commenterId: number,
    public commenter: Users,
    public originalTweet: Tweets
  ) {}
}
