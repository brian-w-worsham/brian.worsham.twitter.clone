export type UserProfileModel = {
  $id: string;
  UserId: number;
  UserName: string;
  Bio: string;
  ProfilePictureUrl: string;
  FormFile: any;
  FollowersCount: number;
  FollowingCount: number;
  UserIsViewingOwnProfile: boolean;
  CurrentUserIsFollowing: boolean;
  FollowId: number;
  ErrorNotification: string;
  TweeterProfilePictureUrls: {
    $id: string;
    $values: Array<{
      $id: string;
      TweetId: number;
      ProfilePictureUrl: string;
    }>;
  };
  RetweeterProfilePictureUrls: {
    $id: string;
    $values: Array<{
      $id: string;
      TweetId: number;
      ProfilePictureUrl: string;
    }>;
  };
  LikedProfilePictureUrls: {
    $id: string;
    $values: Array<{
      $id: string;
      TweetId: number;
      ProfilePictureUrl: string;
    }>;
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
        $values: Array<{
          $ref: string;
        }>;
      };
      Comments: {
        $id: string;
        $values: Array<any>;
      };
      Retweets: {
        $id: string;
        $values: Array<{
          $id?: string;
          Id?: number;
          OriginalTweetId?: number;
          ReTweetCreationDateTime?: string;
          RetweeterId?: number;
          OriginalTweet?: {
            $id: string;
            Id: number;
            Content: string;
            CreationDateTime: string;
            TweeterId: number;
            Tweeter: {
              $id: string;
              Id: number;
              UserName: string;
              Email: string;
              Password: string;
              Bio: string;
              ProfilePictureUrl: string;
              UserRole: string;
              Likes: {
                $id: string;
                Id: number;
                LikedTweetId: number;
                UserThatLikedTweetId: number;
                UserThatLikedTweet: {
                  $ref: string;
                };
                LikedTweet: {
                  $id: string;
                  Id: number;
                  Content: string;
                  CreationDateTime: string;
                  TweeterId: number;
                  Tweeter: {
                    $ref: string;
                  };
                  Comments: {
                    $id: string;
                    $values: Array<any>;
                  };
                  Likes: {
                    $id: string;
                    $values: Array<{
                      $ref: string;
                    }>;
                  };
                  ReTweets: {
                    $id: string;
                    $values: Array<any>;
                  };
                };
              };
              Comments: {
                $id: string;
                $values: Array<any>;
              };
              FollowsFollowedUser: {
                $id: string;
                $values: Array<any>;
              };
              FollowsFollowerUser: {
                $id: string;
                $values: Array<any>;
              };
              ReTweets: {
                $id: string;
                $values: Array<{
                  $id?: string;
                  Id?: number;
                  OriginalTweetId?: number;
                  ReTweetCreationDateTime?: string;
                  RetweeterId?: number;
                  OriginalTweet?: {
                    $id: string;
                    Id: number;
                    Content: string;
                    CreationDateTime: string;
                    TweeterId: number;
                    Tweeter: {
                      $id?: string;
                      Id?: number;
                      UserName?: string;
                      Email?: string;
                      Password?: string;
                      Bio?: string;
                      ProfilePictureUrl?: string;
                      UserRole?: string;
                      Likes: any;
                      Comments?: {
                        $id: string;
                        $values: Array<any>;
                      };
                      FollowsFollowedUser?: {
                        $id: string;
                        $values: Array<any>;
                      };
                      FollowsFollowerUser?: {
                        $id: string;
                        $values: Array<any>;
                      };
                      ReTweets?: {
                        $id: string;
                        $values: Array<any>;
                      };
                      Tweets?: {
                        $id: string;
                        $values: Array<{
                          $ref?: string;
                          $id?: string;
                          Id?: number;
                          Content?: string;
                          CreationDateTime?: string;
                          TweeterId?: number;
                          Tweeter?: {
                            $ref: string;
                          };
                          Comments?: {
                            $id: string;
                            $values: Array<any>;
                          };
                          Likes?: {
                            $id: string;
                            $values: Array<any>;
                          };
                          ReTweets?: {
                            $id: string;
                            $values: Array<{
                              $id: string;
                              Id: number;
                              OriginalTweetId: number;
                              ReTweetCreationDateTime: string;
                              RetweeterId: number;
                              OriginalTweet: {
                                $ref: string;
                              };
                              Retweeter: {
                                $ref: string;
                              };
                            }>;
                          };
                        }>;
                      };
                      $ref?: string;
                    };
                    Comments: {
                      $id: string;
                      $values: Array<any>;
                    };
                    Likes: {
                      $id: string;
                      $values: Array<any>;
                    };
                    ReTweets: {
                      $id: string;
                      $values: Array<{
                        $ref: string;
                      }>;
                    };
                  };
                  Retweeter?: {
                    $ref: string;
                  };
                  $ref?: string;
                }>;
              };
              Tweets: {
                $id: string;
                $values: Array<{
                  $id?: string;
                  Id?: number;
                  Content?: string;
                  CreationDateTime?: string;
                  TweeterId?: number;
                  Tweeter?: {
                    $ref: string;
                  };
                  Comments?: {
                    $id: string;
                    $values: Array<any>;
                  };
                  Likes?: {
                    $id: string;
                    $values: Array<any>;
                  };
                  ReTweets?: {
                    $id: string;
                    $values: Array<any>;
                  };
                  $ref?: string;
                }>;
              };
            };
            Comments: {
              $id: string;
              $values: Array<any>;
            };
            Likes: {
              $id: string;
              $values: Array<any>;
            };
            ReTweets: {
              $id: string;
              $values: Array<{
                $ref: string;
              }>;
            };
          };
          Retweeter?: {
            $ref: string;
          };
          $ref?: string;
        }>;
      };
    }>;
  };
  LikedTweets: any;
  ReTweets: any;
  RetweetedTweets: {
    $id: string;
    $values: Array<{
      $id: string;
      OriginalTweet: {
        $ref: string;
      };
      OriginalTweetId: number;
      OriginalUserName: string;
      OriginalProfilePictureUrl: string;
      OriginalTweetCreationDateTime: string;
      OriginalTweetContent: string;
    }>;
  };
  LikedTweetInfos: {
    $id: string;
    $values: Array<{
      $id: string;
      LikedTweet: {
        $id?: string;
        Id?: number;
        Content?: string;
        CreationDateTime?: string;
        TweeterId?: number;
        Tweeter?: {
          $id: string;
          Id: number;
          UserName: string;
          Email: string;
          Password: string;
          Bio: any;
          ProfilePictureUrl: any;
          UserRole: string;
          Likes: any;
          Comments: {
            $id: string;
            $values: Array<any>;
          };
          FollowsFollowedUser: {
            $id: string;
            $values: Array<any>;
          };
          FollowsFollowerUser: {
            $id: string;
            $values: Array<any>;
          };
          ReTweets: {
            $id: string;
            $values: Array<any>;
          };
          Tweets: {
            $id: string;
            $values: Array<{
              $ref: string;
            }>;
          };
        };
        Comments?: {
          $id: string;
          $values: Array<any>;
        };
        Likes?: {
          $id: string;
          $values: Array<any>;
        };
        ReTweets?: {
          $id: string;
          $values: Array<any>;
        };
        $ref?: string;
      };
      LikedTweetId: number;
      OriginalUserName: string;
      OriginalProfilePictureUrl: string;
      OriginalTweetCreationDateTime: string;
      OriginalTweetContent: string;
    }>;
  };
};
