import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ShortUrl } from '../models/short-url';

@Component({
  selector: 'app-short-url-table',
  imports: [DatePipe],
  templateUrl: './short-url-table.html',
  styleUrl: './short-url-table.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortUrlTable {
  readonly urls = input.required<ShortUrl[]>();

  /** Страница деталей закрыта для анонимов, поэтому им не показываем и ссылку на неё. */
  readonly detailsAvailable = input(false);

  readonly deleteRequested = output<ShortUrl>();
}
