export type TweetsFeedModel = {
  $id: string;
  HasErrors: boolean;
  ValidationErrors: {
    $id: string;
    $values: Array<any>;
  };
  Tweets: {
    $id: string;
    $values: Array<{
      $id: string;
      Id: number;
      TimeSincePosted: string;
      TimeAgo: string;
      Content: string;
      TweeterUserId: number;
      TweeterUserName: string;
      TweeterProfilePictureUrl: string;
      Likes: {
        $id: string;
        $values: Array<any>;
      };
      Comments: {
        $id: string;
        $values: Array<any>;
      };
      Retweets: {
        $id: string;
        $values: Array<any>;
      };
    }>;
  };
  Post: {
    $id: string;
    Content: any;
  };
  currentUserId: number;
  ErrorNotification: string;
};
