export interface ShortUrl {
  id: number;
  originalUrl: string;
  code: string;
  shortUrl: string;
  createdBy: string;
  createdAtUtc: string;
  clickCount: number;
  lastAccessedAtUtc: string | null;
  canDelete: boolean;
}

export interface Session {
  isAuthenticated: boolean;
  userName: string | null;
  isAdmin: boolean;
  antiforgeryToken: string;
}
