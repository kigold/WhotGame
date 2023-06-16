import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, lastValueFrom } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class HeaderInterceptor implements HttpInterceptor {

  constructor(private authService: AuthService) {}

  intercept(httpRequest: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> | any{
      return this.handle(httpRequest, next);
  }

  async handle(httpRequest: HttpRequest<any>, next: HttpHandler){
    if (httpRequest.url.includes("connect/token"))
      return lastValueFrom(next.handle(httpRequest));

    let jwt;
    if (this.authService.isTokenExpired()){
      var tokenResponse = await lastValueFrom(this.authService.refreshAccessToken());
      this.authService.storeAuthInLocalStorage(tokenResponse);
      jwt = tokenResponse.access_token;
    }
    else{
        jwt = this.authService.getToken();
    }
    return lastValueFrom(next.handle(httpRequest.clone({ setHeaders: { authorization: `Bearer ${jwt}`}})));
  }
}
