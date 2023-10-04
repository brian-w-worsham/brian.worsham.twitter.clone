import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { CreateAccountComponent } from './create-account/create-account.component';
import { FooterComponent } from './footer/footer.component';
import { FormsModule } from '@angular/forms';
import { UnauthenticatedLayoutComponent } from './unauthenticated-layout/unauthenticated-layout.component';
import { HttpClientModule } from '@angular/common/http';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { NgModule } from '@angular/core';
import { LoginComponent } from './login/login.component';
import { AuthenticatedLayoutComponent } from './authenticated-layout/authenticated-layout.component';
import { SuggestedUsersPanelComponent } from './suggested-users-panel/suggested-users-panel.component';
import { TweetFormComponent } from './tweet-form/tweet-form.component';
import { TweetListComponent } from './tweet-list/tweet-list.component';
import { TweetActionsRowComponent } from './tweet-actions-row/tweet-actions-row.component';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { CreateCommentComponent } from './create-comment/create-comment.component';
import { TweetAndRelatedCommentsComponent } from './tweet-and-related-comments/tweet-and-related-comments.component';
import { EditProfileComponent } from './edit-profile/edit-profile.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    UnauthenticatedLayoutComponent,
    FooterComponent,
    CreateAccountComponent,
    LoginComponent,
    AuthenticatedLayoutComponent,
    SuggestedUsersPanelComponent,
    TweetFormComponent,
    TweetListComponent,
    TweetActionsRowComponent,
    UserProfileComponent,
    CreateCommentComponent,
    TweetAndRelatedCommentsComponent,
    EditProfileComponent,
  ],
  imports: [
    BrowserModule,
    CommonModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule,
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
