import { NgModule } from '@angular/core';
// imports RouterModule and Routes so the application can have routing capability
import { RouterModule, Routes } from '@angular/router';
// HeroesComponent, gives the Router somewhere to go once you configure the routes.
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';

//Routes tell the Router which view to display when a user clicks a link or pastes a URL into the browser address bar.
const routes: Routes = [
  //A typical Angular Route has two properties:
  //path:	A string that matches the URL in the browser address bar.
  //component:	The component that the router should create when navigating to this route.
  /*When the application starts, the browser's address bar points to the web site's root. That doesn't match any existing route so the router doesn't navigate anywhere. The space below the <router-outlet> is blank. To make the application navigate to the dashboard automatically, add the following route to the routes array.*/
  {
    path: '',
    component: HomeComponent,
    pathMatch: 'full',
    title: "Twitter. it's what's happening / X",
  },
  { path: 'counter', component: CounterComponent, title: 'Counter App' },
  //The colon : character in the path indicates that :id is a placeholder for a specific hero id.
  // { path: 'detail/:id', component: HeroDetailComponent },
  // { path: 'heroes', component: HeroesComponent },
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
