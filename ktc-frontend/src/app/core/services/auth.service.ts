import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, tap, throwError } from 'rxjs';

export interface LoginResponse {
  token: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = 'http://localhost:5033/api/auth/login';
  
  // Utilisation des Signals d'Angular pour un state réactif
  private readonly tokenSignal = signal<string | null>(localStorage.getItem('jwt_token'));
  
  public isAuthenticated = computed(() => this.tokenSignal() !== null);
  public currentUserRoles = computed(() => {
    const token = this.tokenSignal();
    if (!token) return [];
    
    try {
      // Décodage natif du JWT (sans librairie)
      const payload = JSON.parse(atob(token.split('.')[1]));
      // Le back-end .NET met souvent les rôles dans http://.../claims/role
      const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role || payload.roles;
      
      if (Array.isArray(roleClaim)) return roleClaim;
      if (typeof roleClaim === 'string') return [roleClaim];
      return [];
    } catch {
      return [];
    }
  });

  constructor(private http: HttpClient, private router: Router) {}

  login(credentials: { username: string; password: string }) {
    // Si l'URL https du swagger est la bonne, forcez https://localhost:44307/api/Auth/login
    return this.http.post<LoginResponse>('https://localhost:44307/api/Auth/login', credentials)
      .pipe(
        tap(response => {
          if (response && response.token) {
            localStorage.setItem('jwt_token', response.token);
            this.tokenSignal.set(response.token);
            this.router.navigate(['/dashboard']);
          }
        }),
        catchError(error => {
          return throwError(() => new Error('Identifiants invalides ou problème réseau'));
        })
      );
  }

  logout() {
    localStorage.removeItem('jwt_token');
    this.tokenSignal.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.tokenSignal();
  }
}
