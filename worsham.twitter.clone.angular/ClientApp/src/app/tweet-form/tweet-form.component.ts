import { Component } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { NgModel, PatternValidator } from '@angular/forms';
import { Observable, catchError, of } from 'rxjs';
import { Post } from '../models/postModel';

@Component({
  selector: 'app-tweet-form',
  templateUrl: './tweet-form.component.html',
  styleUrls: ['./tweet-form.component.css'],
})
export class TweetFormComponent {
  model = new Post('');
  http!: HttpClient;
  successfulTweetPost: boolean = true;

  private httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
  };

  constructor(http: HttpClient) {
    this.http = http;
  }

  postTweet():void {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = {
        headers: new HttpHeaders({
          'Content-Type': 'application/json',
         'Authorization': `Bearer ${token}`,
        }),
      };
      this.http
        .post(
          'https://localhost:7232/api/tweets/create',
          this.model,
          httpOptions
        )
        .pipe(catchError(this.handleError<any>('postTweet')))
        .subscribe({
          next: (response) => {
            if (response.success == false) {
              this.successfulTweetPost = false;
              document.querySelector('#tweetPostErrorMessage')!.innerHTML =
                response.errorMessage;
              console.log(response.errorMessage);
            } else {
              this.successfulTweetPost = true;
              document.querySelector('#tweetPostErrorMessage')!.innerHTML = '';
              console.info('successfully posted tweet');
              console.log(response);
              window.location.href = '/home';
            }
          },
          error: (error) => {
            this.successfulTweetPost = false;
            document.querySelector('#tweetPostErrorMessage')!.innerHTML =
              error.message;
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
}
