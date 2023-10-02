import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, catchError, of } from 'rxjs';
import { UserProfileModel } from '../models/profileModel';
import { Modal } from 'bootstrap';
import * as bootstrap from 'bootstrap';

@Component({
  selector: 'app-user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.css']
})
export class UserProfileComponent implements OnInit {
  TweeterUserId: string | undefined;
  userProfileModel: UserProfileModel | undefined;
  btnTweets: any;
  btnReTweets: any;
  btnLikes: any;
  panelTweets: any;
  panelReTweets: any;
  panelLikes: any;

  constructor(private route: ActivatedRoute, private http: HttpClient) { }

  ngOnInit(): void {
    this.TweeterUserId = this.route.snapshot.paramMap.get('id?') ?? undefined;
    console.log(this.TweeterUserId);

    this.getUserProfile();
    this.btnTweets = document.getElementById('btnTweets');
    this.btnReTweets = document.getElementById('btnReTweets');
    this.btnLikes = document.getElementById('btnLikes');
    this.panelTweets = document.getElementById('panelTweets');
    this.panelReTweets = document.getElementById('panelReTweets');
    this.panelLikes = document.getElementById('panelLikes');

    if (this.btnTweets && this.btnReTweets && this.btnLikes && this.panelTweets && this.panelReTweets && this.panelLikes) {
      this.addEventListeners();
    }
  }

  addEventListeners() {
    this.btnTweets.addEventListener('click', (event: any) => {
      this.showPanel(this.panelTweets);
      this.toggleButtonActive(this.btnTweets);
    });

    this.btnReTweets.addEventListener('click', (event: any) => {
      this.showPanel(this.panelReTweets);
      this.toggleButtonActive(this.btnReTweets);
    });

    this.btnLikes.addEventListener('click', (event: any) => {
      this.showPanel(this.panelLikes);
      this.toggleButtonActive(this.btnLikes);
    });
  }

  showPanel(panel: any) {
    const panels = [this.panelTweets, this.panelReTweets, this.panelLikes];
    panels.forEach(
      (p) => {
        if (p === panel) {
          p.classList.remove('d-none');
        }
        else {
          p.classList.add('d-none');
        }
      });
  }

  toggleButtonActive(button: any) {
    const buttons = [this.btnTweets, this.btnReTweets, this.btnLikes];
    buttons.forEach(
      (btn) => {
        if (btn === button) {
          btn.classList.add('active');
        }
        else {
          btn.classList.remove('active');
        }
      });
  }

  getUserProfile(): void {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = this.setHttpOptions(token);
      let id: number = 0;
      let url: string = '';
      if (this.TweeterUserId) {
        id = parseInt(this.TweeterUserId);
        url = `https://localhost:7232/api/users/get_profile/${id}`;
      } else {
        url = 'https://localhost:7232/api/users/get_profile';
      }

      this.http
        .get<UserProfileModel>(url, httpOptions)
        .pipe(catchError(this.handleError<any>('getUserProfile')))
        .subscribe({
          next: (response) => {
            if (response.success == false) {
              console.log(response.errorMessage);
            } else {
              console.info('successfully retrieved profile data');
              console.log(response);
              this.userProfileModel = response;
              console.log("UserProfileModel:");
              console.log(this.userProfileModel);
              //window.location.href = '/home';
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
