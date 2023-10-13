export class TweetAndRelatedCommentsViewModel {
  constructor(
    public tweetOwnerName: string,
    public tweetOwnersProfilePicture: string,
    public tweetContent: string,
    public tweetCreationDateTime: string,
    public tweetComments: CommentModelView[]
    ) {}
}

export class CommentModelView {
  constructor(
    public commentId: number,
    public commenterId: number,
    public commenterUserName: string,
    public commentersProfilePicture: string,
    public commentContent: string
  ){}
}
