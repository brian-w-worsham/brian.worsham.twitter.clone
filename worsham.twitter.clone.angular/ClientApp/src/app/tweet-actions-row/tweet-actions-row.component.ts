import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, Input } from '@angular/core';
import { Observable, catchError, of } from 'rxjs';
import { TweetActionsRowModel } from '../models/tweetActionsRowModel';
import { TweetsFeedModel } from '../models/tweetsFeedModel';
import { TweetModel } from '../models/tweetModel';
import { Modal } from 'bootstrap';
import * as bootstrap from 'bootstrap';

@Component({
  selector: 'app-tweet-actions-row',
  templateUrl: './tweet-actions-row.component.html',
  styleUrls: ['./tweet-actions-row.component.css'],
})
export class TweetActionsRowComponent {
  model!: TweetActionsRowModel[];
  tweetsFeed!: TweetsFeedModel;
  @Input() commentsCount!: number;
  @Input() likesCount!: number;
  @Input() reTweetsCount!: number;
  @Input() index: number | undefined;

  constructor(private http: HttpClient) {}

  ngOnInit() {}

  postLike(tweetId: number): void {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = this.setHttpOptions(token);
      this.http
        .post(
          'https://localhost:7232/api/likes/like_tweet',
          tweetId,
          httpOptions
        )
        .pipe(catchError(this.handleError<any>('postTweet')))
        .subscribe({
          next: (result) => {
            console.log(result);
          },
          error: (error) => console.error(error),
        });
    }
  }

  postRetweet(tweetId: number): void {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = this.setHttpOptions(token);
      this.http
        .post(
          'https://localhost:7232/api/retweets/retweet_tweet',
          tweetId,
          httpOptions
        )
        .pipe(catchError(this.handleError<any>('postTweet')))
        .subscribe({
          next: (result) => {
            console.log(result);
          },
          error: (error) => console.error(error),
        });
    }
  }

  postReply(tweetId: number): void {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = this.setHttpOptions(token);
      this.http
        .post(
          'https://localhost:7232/api/replies/reply_tweet',
          tweetId,
          httpOptions
        )
        .pipe(catchError(this.handleError<any>('postTweet')))
        .subscribe({
          next: (result) => {
            console.log(result);
          },
          error: (error) => console.error(error),
        });
    }
  }

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(error);
      //debugger;
      return of(result as T);
    };
  }

  private setHttpOptions(token: string) {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      }),
    };
    return httpOptions;
  }
}
