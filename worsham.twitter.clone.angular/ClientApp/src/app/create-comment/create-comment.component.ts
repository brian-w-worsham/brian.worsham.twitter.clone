import { Component, Input } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { NgModel } from '@angular/forms';
import { Modal } from 'bootstrap';
import * as bootstrap from 'bootstrap';
import { TweetModel } from '../models/tweetModel';
import { TweetsFeedModel } from '../models/tweetsFeedModel';
import { Observable, catchError, of } from 'rxjs';
import { CommentsModel } from '../models/createCommentModel';

@Component({
  selector: 'app-create-comment',
  templateUrl: './create-comment.component.html',
  styleUrls: ['./create-comment.component.css'],
})
export class CreateCommentComponent {
  @Input() index!: number;
  @Input() tweetsFeed!: TweetsFeedModel;
  successfulCommentPost: boolean = true;
  model!: CommentsModel;

  constructor(private http: HttpClient) {}

  postComment(
    originalTweetId: string,
    currentUserId: string,
    postCommentContent: string
  ): void {
    this.model = {
      Content: postCommentContent,
      OriginalTweetId: parseInt(originalTweetId),
      CommenterId: parseInt(currentUserId),
    };
    const token = localStorage.getItem('jwtToken');
    if (
      token &&
      this.model.CommenterId &&
      this.model.OriginalTweetId &&
      this.model.Content
    ) {
      const httpOptions = this.setHttpOptions(token);
      this.http
        .post(
          'https://localhost:7232/api/comments/create',
          this.model,
          httpOptions
        )
        .pipe(catchError(this.handleError<any>('postComment')))
        .subscribe({
          next: (response) => {
            if (response.success == false) {
              console.log(response.errorMessage);
              this.successfulCommentPost = false;
            } else {
              console.info('successfully posted tweet');
              console.log(response);
              this.successfulCommentPost = true;
              //this.getNotFollowedUsers();
              window.location.href = '/home';
            }
          },
          error: (error) => {
            this.successfulCommentPost = false;
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
