import { Component } from '@angular/core';
import { AuthService, RegisterDto } from '../auth';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  imports: [FormsModule, CommonModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  model: RegisterDto = { fullName: '', email: '', password: '' };
  error = '';

  constructor(private readonly authService: AuthService, private readonly router: Router) { }

  register() {
    this.authService.register(this.model).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: (err) => this.error = 'Failed register'
    });
  }

}
