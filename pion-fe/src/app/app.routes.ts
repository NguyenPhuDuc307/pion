import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./home/home').then(m => m.Home)
  },
  {
    path: 'weather',
    loadComponent: () =>
      import('./weather/weather').then(m => m.WeatherComponent)
  },
  {
    path: 'auth/login',
    loadComponent: () =>
      import('../api-authorization/login/login').then(m => m.LoginComponent)
  },
  {
    path: 'auth/register',
    loadComponent: () =>
      import('../api-authorization/register/register').then(m => m.RegisterComponent)
  },
];
