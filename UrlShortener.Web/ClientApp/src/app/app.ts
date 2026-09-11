import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal, viewChild } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AddUrlForm } from './components/add-url-form';
import { ShortUrlTable } from './components/short-url-table';
import { ShortUrl } from './models/short-url';
import { SessionService } from './services/session.service';
import { ShortUrlApiService } from './services/short-url-api.service';

@Component({
  selector: 'app-root',
  imports: [AddUrlForm, ShortUrlTable],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App implements OnInit {
  private readonly api = inject(ShortUrlApiService);
  private readonly addForm = viewChild(AddUrlForm);

  protected readonly session = inject(SessionService);
  protected readonly urls = signal<ShortUrl[]>([]);
  protected readonly loading = signal(true);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    forkJoin({ session: this.session.load(), urls: this.api.getAll() }).subscribe({
      next: ({ urls }) => {
        this.urls.set(urls);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.error.set(this.describe(error));
        this.loading.set(false);
      },
    });
  }

  protected create(url: string): void {
    this.pending.set(true);
    this.error.set(null);

    this.api.create(url).subscribe({
      next: (created) => {
        // Список правится на месте: перезагрузка страницы требованиям противоречит,
        // а повторный GET всего списка ради одной строки избыточен.
        this.urls.update((current) => [created, ...current]);
        this.addForm()?.reset();
        this.pending.set(false);
      },
      error: (error: unknown) => {
        this.error.set(this.describe(error));
        this.pending.set(false);
      },
    });
  }

  protected remove(url: ShortUrl): void {
    if (!confirm(`Удалить ссылку /s/${url.code}?`)) {
      return;
    }

    this.error.set(null);

    this.api.delete(url.id).subscribe({
      next: () => this.urls.update((current) => current.filter((item) => item.id !== url.id)),
      error: (error: unknown) => this.error.set(this.describe(error)),
    });
  }

  private describe(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 401) {
        return 'Сессия истекла. Войдите заново, чтобы продолжить.';
      }

      if (error.status === 403) {
        return 'Недостаточно прав: удалять можно только свои ссылки.';
      }

      const detail = (error.error as { detail?: string } | null)?.detail;

      if (detail) {
        return detail;
      }
    }

    return 'Операция не удалась. Попробуйте ещё раз.';
  }
}
