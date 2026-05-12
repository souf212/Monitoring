import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, tap, throwError } from 'rxjs';

export interface LoginResponse {
  token: string;
  roles: string[];
}

/** Groupes AD utilisés pour le RBAC */
export const AD_ROLES = {
  READ_ONLY:    'Admin_ReadOnly',
  FULL_ACCESS:  'Support_FullAccess'
} as const;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API_URL = 'http://localhost:5239/api/auth/login';

  // ── State réactif ──────────────────────────────────────────────────────────
  private readonly tokenSignal = signal<string | null>(this.getStoredToken());

  public isAuthenticated = computed(() => this.tokenSignal() !== null);

  /** Payload décodé du JWT (mis en cache dans un computed) */
  private readonly jwtPayload = computed(() => {
    const token = this.tokenSignal();
    if (!token) return null;
    try {
      return JSON.parse(atob(token.split('.')[1]));
    } catch {
      return null;
    }
  });

  public currentUser = computed(() => {
    const payload = this.jwtPayload();
    if (!payload) return { username: null as string | null, email: null as string | null, displayName: null as string | null };
    const displayName = payload.name ?? payload.display_name ?? payload.given_name ?? payload.unique_name ?? null;
    const email       = payload.email ?? payload.upn ?? payload.preferred_username ??
                        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ?? null;
    const username    = payload.preferred_username ?? payload.unique_name ?? payload.sub ??
                        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? null;
    return { username, email, displayName };
  });

  /** Tous les rôles de l'utilisateur (groupes AD injectés dans le JWT) */
  public currentUserRoles = computed((): string[] => {
    const payload = this.jwtPayload();
    if (!payload) return [];
    // .NET émet les rôles sous ClaimTypes.Role (URI long) ET sous "role" (court)
    const roleClaim =
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
      payload.role ??
      payload.roles;
    if (Array.isArray(roleClaim)) return roleClaim;
    if (typeof roleClaim === 'string') return [roleClaim];
    return [];
  });

  // ── Signals RBAC ──────────────────────────────────────────────────────────

  /** True si l'utilisateur appartient au groupe AD "Support_FullAccess" */
  public isSupport = computed(() =>
    this.currentUserRoles().includes(AD_ROLES.FULL_ACCESS)
  );

  /** True si l'utilisateur appartient à "Admin_ReadOnly" (sans FullAccess) */
  public isReadOnly = computed(() =>
    this.currentUserRoles().includes(AD_ROLES.READ_ONLY) &&
    !this.currentUserRoles().includes(AD_ROLES.FULL_ACCESS)
  );

  /** True si l'utilisateur peut lire (appartient à au moins un des deux groupes) */
  public canRead = computed(() =>
    this.isSupport() || this.currentUserRoles().includes(AD_ROLES.READ_ONLY)
  );

  /** Vérifie dynamiquement la présence d'un ou plusieurs rôles */
  public hasRole(roles: string[]): boolean {
    return roles.some(r => this.currentUserRoles().includes(r));
  }

  constructor(private http: HttpClient, private router: Router) {}

  private getStoredToken(): string | null {
    const token = localStorage.getItem('jwt_token');
    if (token && token.split('.').length === 3) return token;
    return null;
  }

  login(credentials: { username: string; password: string }) {
    return this.http.post<LoginResponse>(this.API_URL, credentials).pipe(
      tap(response => {
        if (response?.token) {
          localStorage.setItem('jwt_token', response.token);
          this.tokenSignal.set(response.token);
          this.router.navigate(['/dashboard']);
        }
      }),
      catchError(() => throwError(() => new Error('Identifiants invalides ou problème réseau')))
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