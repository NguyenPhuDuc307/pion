import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { AuthService } from '../auth';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login-menu',
  imports: [FormsModule, CommonModule, RouterModule],
  templateUrl: './login-menu.html',
  styleUrl: './login-menu.css'
})
export class LoginMenuComponent implements OnInit, OnDestroy {
  public isAuthenticated = false;
  public userName: string | null = null;
  private authSub?: Subscription;

  constructor(private readonly authorizeService: AuthService, private readonly router: Router) { }

  ngOnInit() {
    this.authSub = this.authorizeService.authState$.subscribe(isAuth => {
      this.isAuthenticated = isAuth;
      const user = this.authorizeService.getCurrentUser();
      this.userName = user ? user.fullName : null;
    });
  }

  ngOnDestroy() {
    this.authSub?.unsubscribe();
  }

  logout() {
    this.authorizeService.logout();
    this.router.navigate(['/']);
  }
}
