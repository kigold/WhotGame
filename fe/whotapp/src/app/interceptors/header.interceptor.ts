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

  private attemptingRefreshingToken: boolean = false;

  intercept(httpRequest: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> | any{
      return this.handle(httpRequest, next);
  }

  async handle(httpRequest: HttpRequest<any>, next: HttpHandler){
    //IF Token has expired and refresh token is not valid
    if (this.attemptingRefreshingToken){
      this.attemptingRefreshingToken = false;
      this.authService.logout();
      return

    }
    if (httpRequest.url.includes("connect/token")){
      return lastValueFrom(next.handle(httpRequest));
    }

    let jwt;
    if (this.authService.isTokenExpired()){

      if (httpRequest.url.includes("connect/token")){        
        this.attemptingRefreshingToken = true;
      }

      var tokenResponse = await lastValueFrom(this.authService.refreshAccessToken());
      this.authService.storeAuthInLocalStorage(tokenResponse);
      jwt = tokenResponse.access_token;
      this.attemptingRefreshingToken = false;
    }
    else{
        jwt = this.authService.getToken();
    }
    return lastValueFrom(next.handle(httpRequest.clone({ setHeaders: { authorization: `Bearer ${jwt}`}})));
  }
}
