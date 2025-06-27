import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../environments/environment';

export interface AuthResponseDto {
  fullName: string;
  email: string;
  token: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  fullName: string;
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseUrl = environment.apiUrl + '/auth';
  private readonly userKey = 'auth_user';
  private readonly authStateSubject = new BehaviorSubject<boolean>(this.isAuthenticated());
  authState$ = this.authStateSubject.asObservable();

  constructor(private readonly http: HttpClient) { }

  login(data: LoginDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/login`, data)
      .pipe(tap(res => {
        localStorage.setItem('token', res.token);
        localStorage.setItem(this.userKey, JSON.stringify({ fullName: res.fullName, email: res.email }));
        this.authStateSubject.next(true);
      })
      );
  }

  register(data: RegisterDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.baseUrl}/register`, data)
      .pipe(tap(res => {
        localStorage.setItem('token', res.token);
        localStorage.setItem(this.userKey, JSON.stringify({ fullName: res.fullName, email: res.email }));
        this.authStateSubject.next(true);
      })
      );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem(this.userKey);
    this.authStateSubject.next(false);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getCurrentUser(): { fullName: string, email: string } | null {
    const json = localStorage.getItem(this.userKey);
    return json ? JSON.parse(json) : null;
  }
}
