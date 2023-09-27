import { Tweets } from './tweets';
import { Users } from './users';

export class Likes {
  constructor(
    public id: number,
    public likedTweetId: number,
    public userThatLikedTweetId: number,
    public userThatLikedTweet: Users,
    public likedTweet: Tweets

  ) {}
}
