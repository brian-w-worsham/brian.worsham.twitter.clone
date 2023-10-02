import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { NgModel, PatternValidator } from '@angular/forms';
import { Modal } from 'bootstrap';
import * as bootstrap from 'bootstrap';
import { Observable, catchError, of } from 'rxjs';

@Component({
  selector: 'app-edit-proile',
  templateUrl: './edit-proile.component.html',
  styleUrls: ['./edit-proile.component.css'],
})
export class EditProileComponent implements OnInit {
  modal!: Modal;
  http!: HttpClient;
  successfulPost: boolean = true;

  constructor(http: HttpClient) {
    this.http = http;
  }

  ngOnInit(): void {
    const editProfileModal = document.getElementById('editProfileModal');
    if (editProfileModal) {
      this.modal = new Modal(editProfileModal);
    }
  }

  updateProfile(): void {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = this.setHttpOptions(token);

      this.http
        .post(
          'https://localhost:7232/api/users/edit',
          // parseInt(tweetId),
          httpOptions,
        )
        .pipe(catchError(this.handleError<any>('updateProfile')))
        .subscribe({
          next: (response) => {
            if (response.success == false) {
              console.log(response.errorMessage);
            } else {
              console.info('successfully updated user profile');
              console.log(response);
              window.location.reload();
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
