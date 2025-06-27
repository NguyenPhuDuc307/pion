import { Component } from '@angular/core';
import { AuthService } from '../../api-authorization/auth';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LoginMenuComponent } from '../../api-authorization/login-menu/login-menu';

@Component({
  selector: 'app-nav-menu',
  imports: [RouterModule, CommonModule, LoginMenuComponent],
  templateUrl: './nav-menu.html',
  styleUrl: './nav-menu.css'
})
export class NavMenu {
  isExpanded = false;

  constructor(public authService: AuthService, private readonly router: Router) { }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
