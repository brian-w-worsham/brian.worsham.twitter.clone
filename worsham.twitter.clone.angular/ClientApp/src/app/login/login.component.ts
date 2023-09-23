import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { NgModel, PatternValidator } from '@angular/forms';
import { Modal } from 'bootstrap';
import * as bootstrap from 'bootstrap';
import { Observable, catchError, of } from 'rxjs';
import { Login } from '../models/login';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  modal!: Modal;
  model = new Login('', '');
  http!: HttpClient;
  successfulPost: boolean = true;
  private httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
  };

  constructor(http: HttpClient) {
    this.http = http;
  }

  ngOnInit(): void {
    const loginModal = document.getElementById('loginModal');
    if (loginModal) {
      this.modal = new Modal(loginModal);
    }
  }

  login(): void {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    this.http
      .post('https://localhost:7232/api/users/login', this.model, {
        headers: headers,
      })
      .pipe(catchError(this.handleError<any>('createUser')))
      .subscribe(
        (response) => {
          if (response.success == false) {
            this.successfulPost = false;
            document.querySelector('#loginErrorMessage')!.innerHTML =
              response.errorMessage;
            console.log(response.errorMessage);
          } else {
            // successfully authenticated user
            this.successfulPost = true;
            document.querySelector('#loginErrorMessage')!.innerHTML = '';
            console.info('successfully authenticated user');
            this.modal.hide();
            console.log(response);
            window.location.href = '/Tweets/Index';
          }
        },
        (error) => {
          this.successfulPost = false;
          document.querySelector('#loginErrorMessage')!.innerHTML =
            error.message;
          console.log(error.message);
        }
      );
  }

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(error);
      debugger;
      return of(result as T);
    };
  }
}
