import { Component } from '@angular/core';
import { TweetsFeedModel } from '../models/tweetsFeedModel';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';

@Component({
  selector: 'app-tweet-list',
  templateUrl: './tweet-list.component.html',
  styleUrls: ['./tweet-list.component.css'],
})
export class TweetListComponent {
  tweetsFeed!: TweetsFeedModel;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = {
        headers: new HttpHeaders({
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        }),
      };

      this.http
        .get<TweetsFeedModel>(
          'https://localhost:7232/api/tweets/get_tweets_feed',
          httpOptions
        )
        .pipe(catchError(this.handleError<any>('postTweet')))
        .subscribe({
          next: (result) => {
            this.tweetsFeed = result;
            console.log(this.tweetsFeed.Tweets);
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
}
