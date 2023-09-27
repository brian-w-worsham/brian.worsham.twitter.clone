import { Comments } from './comments';
import { Follows } from './follows';
import { Likes } from './likes';
import { ReTweets } from './reTweets';
import { Tweets } from './tweets';

export class Users {
  constructor(
    public userName: string,
    public email: string,
    public password: string,
    public userRole: string,
    public bio?: string,
    public profilePictureUrl?: string,
    public id?: number,
    public likes?: Likes[],
    public comments?: Comments[],
    public followsFollowedUser?: Follows[],
    public followsFollowerUser?: Follows[],
    public retweets?: ReTweets[],
    public tweets?: Tweets[]
  ) {}
}
