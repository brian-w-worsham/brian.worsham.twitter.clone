import { Users } from './users';

export class Follows {
  constructor(
    public id: number,
    public followedUserId: number,
    public followerUserId: number,
    public followedUser: Users,
    public followerUser: Users
  ) {}
}
