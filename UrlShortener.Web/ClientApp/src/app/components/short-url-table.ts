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

  /** The details page is closed to anonymous users, so they are not shown the link to it either. */
  readonly detailsAvailable = input(false);

  readonly deleteRequested = output<ShortUrl>();
}
