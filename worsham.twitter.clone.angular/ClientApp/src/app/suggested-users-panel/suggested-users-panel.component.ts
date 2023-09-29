import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component } from '@angular/core';
import { Observable, catchError, of } from 'rxjs';

@Component({
  selector: 'app-suggested-users-panel',
  templateUrl: './suggested-users-panel.component.html',
  styleUrls: ['./suggested-users-panel.component.css'],
})
export class SuggestedUsersPanelComponent {
  suggestedUsers!: {
    userName: string;
    id: number;
  }[];
  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.getNotFollowedUsers();
  }

  getNotFollowedUsers(): void {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = this.setHttpOptions(token);

      this.http
        .get('https://localhost:7232/api/follows/notfollowed', httpOptions)
        .pipe(catchError(this.handleError<any>('getNotFollowedUsers')))
        .subscribe({
          next: (result) => {
            this.suggestedUsers = result;
          },
          error: (error) => console.error(error),
        });
    }
  }

  postFollow(userId: number): void {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = this.setHttpOptions(token);
      this.http
        .post(
          'https://localhost:7232/api/follows/follow_user',
          userId,
          httpOptions
        )
        .pipe(catchError(this.handleError<any>('postFollow')))
        .subscribe({
          next: (response) => {
            if (response.success == false) {
              console.log(response.errorMessage);
            } else {
              console.info('successfully posted tweet');
              console.log(response);
              //this.getNotFollowedUsers();
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
