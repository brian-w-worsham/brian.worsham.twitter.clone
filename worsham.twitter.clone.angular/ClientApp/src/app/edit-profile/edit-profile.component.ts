import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import {
  HttpClient,
  HttpEventType,
  HttpErrorResponse,
  HttpHeaders,
} from '@angular/common/http';
import { NgModel, PatternValidator } from '@angular/forms';
import { Modal } from 'bootstrap';
import * as bootstrap from 'bootstrap';
import { Observable, catchError, of } from 'rxjs';
import { UserProfileModel } from '../models/profileModel';
import { EditProfileModel } from '../models/editProfileModel';

@Component({
  selector: 'app-edit-profile',
  templateUrl: './edit-profile.component.html',
  styleUrls: ['./edit-profile.component.css'],
})
export class EditProfileComponent implements OnInit {
  modal!: Modal;
  // http!: HttpClient;
  successfulPost: boolean = true;
  @Input() userProfileModel!: UserProfileModel;

  progress!: number;
  message: string | undefined;
  // @Output() public onUploadFinished = new EventEmitter();
  @Output() public whenUploadFinished = new EventEmitter();

  constructor(private http: HttpClient) {
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

      const formData = new FormData();
      formData.append('UserId', this.userProfileModel.UserId.toString());
      formData.append('UserName', this.userProfileModel.UserName);
      formData.append('Bio', this.userProfileModel.Bio);
      const fileReader = new FileReader();
      fileReader.readAsArrayBuffer(this.userProfileModel.FormFile);
      fileReader.onload = () => {
        const arrayBuffer = fileReader.result as ArrayBuffer;
        const uint8Array = new Uint8Array(arrayBuffer);
        formData.append(
          'FormFile',
          new Blob([uint8Array], { type: this.userProfileModel.FormFile.type }),
          this.userProfileModel.FormFile
            ? this.userProfileModel.FormFile.name
            : 'profilePic',
        );
      };

      // const editProfileModel = new EditProfileModel(
      //   this.userProfileModel.UserId,
      //   this.userProfileModel.UserName,
      //   this.userProfileModel.Bio,
      //   this.userProfileModel.FormFile,
      // this.userProfileModel.FormFile
      //   ? this.userProfileModel.FormFile.name
      //   : 'profilePic',
      // );

      this.http
        .post('https://localhost:7232/api/users/edit', formData, httpOptions)
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

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    this.userProfileModel.FormFile = file;
  }

  uploadFile = (files: any) => {
    if (files.length === 0) {
      return;
    }

    const token = localStorage.getItem('jwtToken');
    if (token) {
      const httpOptions = {
        // We are generating a random string to use as the boundary parameter, which ensures that the boundary is unique for each request
        headers: new HttpHeaders({
          Authorization: `Bearer ${token}`,
        }),
        reportProgress: true,
        observe: 'events' as const,
      };

      let fileToUpload = <File>files[0];
      const formData = new FormData();
      formData.append('file', fileToUpload, fileToUpload.name);

      this.http
        .post('https://localhost:7232/api/users/upload', formData, httpOptions)
        .pipe(catchError(this.handleError<any>('updateProfile')))
        .subscribe({
          next: (event) => {
            if (event.type === HttpEventType.UploadProgress)
              this.progress = Math.round((100 * event.loaded) / event.total!);
            else if (event.type === HttpEventType.Response) {
              this.message = 'Upload success.';
              this.whenUploadFinished.emit(event.body);
            }
          },
          error: (err: HttpErrorResponse) => console.log(err),
        });
    }
  };

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      if (error instanceof HttpErrorResponse) {
        this.logErrors(error);
      }
      console.error(error);
      //debugger;
      return of(result as T);
    };
  }

  // Assuming 'error' is an instance of HttpErrorResponse
  private logErrors(error: HttpErrorResponse): void {
    if (error.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      console.error('An error occurred:', error.error.message);
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong.
      console.error(
        `Backend returned code ${error.status}, ` +
          `body was: ${JSON.stringify(error.error)}`,
      );
    }
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
