import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { SessionService } from './session.service';

const SAFE_METHODS = new Set(['GET', 'HEAD', 'OPTIONS']);

/**
 * Angular's built-in XSRF mechanism does not fit here: ASP.NET Core puts its token into an
 * HttpOnly cookie that scripts cannot read. The token arrives from /api/session and travels
 * back in the header the server expects.
 */
export const csrfInterceptor: HttpInterceptorFn = (request, next) => {
  if (SAFE_METHODS.has(request.method)) {
    return next(request);
  }

  const token = inject(SessionService).antiforgeryToken;

  return next(token ? request.clone({ setHeaders: { 'X-CSRF-TOKEN': token } }) : request);
};
