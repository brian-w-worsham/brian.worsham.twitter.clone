import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UnauthenticatedLayoutComponent } from './unauthenticated-layout/unauthenticated-layout.component';
import { AuthenticatedLayoutComponent } from './authenticated-layout/authenticated-layout.component';
import { UserProfileComponent } from './user-profile/user-profile.component';

const routes: Routes = [
  {
    path: '',
    component: UnauthenticatedLayoutComponent,
    pathMatch: 'full',
    title: "Twitter. it's what's happening / X",
  },
  { path: 'home', component: AuthenticatedLayoutComponent, title: 'Home / X' },
  { path: 'profile', component: UserProfileComponent, title: 'Profile' },
  { path: 'profile/:id?', component: UserProfileComponent, title: 'Profile' },
];

//The @NgModule metadata initializes the router and starts it listening for browser location changes.
@NgModule({
  //The following line adds the RouterModule to the AppRoutingModule imports array and configures it with the routes in one step by calling RouterModule.forRoot():
  //The method is called forRoot() because you configure the router at the application's root level. The forRoot() method supplies the service providers and directives needed for routing, and performs the initial navigation based on the current browser URL.
  imports: [RouterModule.forRoot(routes)],
  // Next, AppRoutingModule exports RouterModule to be available throughout the application.
  exports: [RouterModule],
})
export class AppRoutingModule {}
