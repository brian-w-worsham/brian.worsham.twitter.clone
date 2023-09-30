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
  @Input() tweetsFeed!: TweetsFeedModel;
  @Input() index!: number;
  @Input() commentsCount!: number;
  @Input() likesCount!: number;
  @Input() reTweetsCount!: number;

  constructor(private http: HttpClient) { }

  postLike(tweetId: string): void {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = this.setHttpOptions(token);

      this.http
        .post('https://localhost:7232/api/likes/create', parseInt(tweetId), httpOptions)
        .pipe(catchError(this.handleError<any>('postLike')))
        .subscribe({
          next: (response) => {
            if (response.success == false) {
              console.log(response.errorMessage);
            } else {
              console.info('successfully liked a post');
              console.log(response);
              window.location.href = '/home';
            }
          },
          error: (error) => {
            console.log(error.message);
          },
        });
    }
  }

  postRetweet(tweetId: string): void {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = this.setHttpOptions(token);

      this.http
        .post(
          'https://localhost:7232/api/retweets/create', parseInt(tweetId), httpOptions)
        .pipe(catchError(this.handleError<any>('postReTweet')))
        .subscribe({
          next: (response) => {
            if (response.success == false) {
              console.log(response.errorMessage);
            } else {
              console.info('successfully posted reTweet');
              console.log(response);
              window.location.href = '/home';
            }
          },
          error: (error) => {
            console.log(error.message);
          },
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
