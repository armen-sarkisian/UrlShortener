import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { Session } from '../models/short-url';

/**
 * Who is signed in right now. The server returns this together with the antiforgery token,
 * so a single request on start-up covers both permissions and protected operations.
 */
@Injectable({ providedIn: 'root' })
export class SessionService {
  private readonly http = inject(HttpClient);
  private readonly state = signal<Session | null>(null);

  readonly session = this.state.asReadonly();
  readonly isAuthenticated = computed(() => this.state()?.isAuthenticated ?? false);
  readonly userName = computed(() => this.state()?.userName ?? null);

  get antiforgeryToken(): string {
    return this.state()?.antiforgeryToken ?? '';
  }

  load(): Observable<Session> {
    return this.http.get<Session>('/api/session').pipe(tap((session) => this.state.set(session)));
  }
}
