import { Component } from '@angular/core';
import { AuthService, LoginDto } from '../auth';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  model: LoginDto = { email: '', password: '' };
  error = '';
  baseUrl = environment.apiUrl;

  constructor(private readonly authService: AuthService, private readonly router: Router) { }

  login() {
    this.authService.login(this.model).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: (err) => this.error = 'Failed login'
    });
  }
}
