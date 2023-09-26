import { Component, OnInit, Inject, ViewChild } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { NgModel, PatternValidator } from '@angular/forms';
import { Modal } from 'bootstrap';
import * as bootstrap from 'bootstrap';
import { Observable, catchError, of } from 'rxjs';
import { User } from '../models/user';

@Component({
  selector: 'app-create-account',
  templateUrl: './create-account.component.html',
  styleUrls: ['./create-account.component.css'],
})
export class CreateAccountComponent implements OnInit {
  modal!: Modal;
  model = new User('', '', '', 'newuser');
  passwordPattern = '^(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*]).{8,12}$';
  http!: HttpClient;
  private httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
  };
  @ViewChild('userName') userName!: NgModel;
  @ViewChild('email') email!: NgModel;

  constructor(http: HttpClient) {
    this.http = http;
  }

  ngOnInit(): void {
    const createAccountModal = document.getElementById('createAccountModal');
    if (createAccountModal) {
      this.modal = new Modal(createAccountModal);
    }
    const tooltipTriggerList = [].slice.call(
      document.querySelectorAll('[data-bs-toggle="tooltip"]')
    );
    if (tooltipTriggerList) {
      const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
      });
    }
  }

  createUser(): void {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    //debugger;
    this.http
      .post('https://localhost:7232/api/users/create', this.model, {
        headers: headers,
      })
      .pipe(catchError(this.handleError<any>('createUser')))
      .subscribe({
        next: (response) => {
          if (response.success == false) {
            //update the UI to show the error
            switch (response.errorMessage) {
              case `The name, ${this.model.userName}, is already taken.`:
                this.userName.control.setErrors({ invalid: true });
                const userNameErrorMsgErrorDiv =
                  document.getElementById('userNameErrorMsg');
                if (userNameErrorMsgErrorDiv) {
                  userNameErrorMsgErrorDiv.innerHTML = response.errorMessage;
                }
                break;
              case `The email address, ${this.model.email}, is already taken.`:
                this.email.control.setErrors({ invalid: true });
                const emailErrorDiv = document.getElementById('emailErrorMsg');
                if (emailErrorDiv) {
                  emailErrorDiv.innerHTML = response.errorMessage;
                }
                break;
            }
            console.log(response.errorMessage);
          } else {
            // successfully registered user
            // clear all the modal's input values so it has a clean slate when it's opened again
            const modalInputs = document.querySelectorAll(
              '#createAccountForm input'
            );
            if (modalInputs) {
              modalInputs.forEach((input) => {
                const inputElement = input as HTMLInputElement;
                // if inputElement has "userRole" as it's id skip it
                if (inputElement.id !== 'userRole') {
                  inputElement.value = '';
                }
              });
            }
            // close registration modal and open login modal
            this.modal.hide();
            console.log(
              `Successfully registered user ${this.model.userName} - ${response}`
            );
            document.getElementById('btnSignIn')?.click();
          }
        },
        error: (error) => {
          console.log(error.message);
        },
      });
  }

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(error);
      //debugger;
      return of(result as T);
    };
  }
}
