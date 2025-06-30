import { Routes } from '@angular/router';
import { ProductListComponent } from './products/product-list/product-list';
import { ProductFormComponent } from './products/product-form/product-form';

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
    path: 'products',
    component: ProductListComponent
  },
  {
    path: 'products/new',
    component: ProductFormComponent
  },
  {
    path: 'products/edit/:id',
    component: ProductFormComponent
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
