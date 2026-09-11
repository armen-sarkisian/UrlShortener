import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { SessionService } from './session.service';

const SAFE_METHODS = new Set(['GET', 'HEAD', 'OPTIONS']);

/**
 * Штатный XSRF-механизм Angular здесь не подходит: ASP.NET Core кладёт свой токен
 * в HttpOnly-куку, которую скрипт прочитать не может. Токен приходит из /api/session
 * и уезжает обратно заголовком, который ожидает сервер.
 */
export const csrfInterceptor: HttpInterceptorFn = (request, next) => {
  if (SAFE_METHODS.has(request.method)) {
    return next(request);
  }

  const token = inject(SessionService).antiforgeryToken;

  return next(token ? request.clone({ setHeaders: { 'X-CSRF-TOKEN': token } }) : request);
};
