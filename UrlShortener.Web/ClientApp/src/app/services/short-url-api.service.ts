import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ShortUrl } from '../models/short-url';

@Injectable({ providedIn: 'root' })
export class ShortUrlApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/shorturls';

  getAll(): Observable<ShortUrl[]> {
    return this.http.get<ShortUrl[]>(this.baseUrl);
  }

  create(url: string): Observable<ShortUrl> {
    return this.http.post<ShortUrl>(this.baseUrl, { url });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
