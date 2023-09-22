import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { PatternValidator } from '@angular/forms';
import { Modal } from 'bootstrap';
import { User } from '../models/user';
import * as bootstrap from 'bootstrap';
import { Observable, catchError, of } from 'rxjs';

@Component({
  selector: 'app-create-account',
  templateUrl: './create-account.component.html',
  styleUrls: ['./create-account.component.css'],
})
export class CreateAccountComponent implements OnInit {
  modal!: Modal;
  model = new User('', '', '', 'newuser');
  submitted = false;
  passwordPattern = '^(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*]).{8,12}$';
  http!: HttpClient;
  private httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
  };

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
    // this.submitted = true;
    console.log(`User: ${this.model.userName} created as ${this.model}!`);
    //debugger;
    this.http
      .post('https://localhost:7232/api/Users', this.model, {
        headers: headers,
      })
      .subscribe((response) => {
        console.log(response);
      });
  }

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(error);
      return of(result as T);
    };
  }
}
