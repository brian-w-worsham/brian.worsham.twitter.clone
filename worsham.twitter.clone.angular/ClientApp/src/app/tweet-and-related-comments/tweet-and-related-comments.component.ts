import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { TweetAndRelatedCommentsViewModel } from '../models/tweetAndRelatedCommentsViewModel';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-tweet-and-related-comments',
  templateUrl: './tweet-and-related-comments.component.html',
  styleUrls: ['./tweet-and-related-comments.component.css'],
})
export class TweetAndRelatedCommentsComponent implements OnInit {
  tweetId: string | undefined;
  model!: TweetAndRelatedCommentsViewModel;

  constructor(
    private route: ActivatedRoute,
    private http: HttpClient,
  ) {}

  ngOnInit() {
    this.tweetId = this.route.snapshot.paramMap.get('id?') ?? undefined;
    this.getTweetAndRelatedComments();
  }

  getTweetAndRelatedComments(): void {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = this.setHttpOptions(token);
      let tweetId: number = 0;
      let url: string = '';
      tweetId = parseInt(this.tweetId ?? '0');
      url = `https://localhost:7232/api/tweets/tweet_and_related_comments/${tweetId}`;

      this.http
        .get<TweetAndRelatedCommentsViewModel>(url, httpOptions)
        .pipe(catchError(this.handleError<any>('getTweetAndRelatedComments')))
        .subscribe({
          next: (response) => {
            if (response.success == false) {
              console.log(response.errorMessage);
            } else {
              this.model = {
                tweetOwnerName: response.tweetOwnerName,
                tweetOwnersProfilePicture: response.tweetOwnersProfilePicture,
                tweetContent: response.tweetContent,
                tweetCreationDateTime: response.tweetCreationDateTime,
                tweetComments: response.tweetComments.$values,
              };
            }
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
