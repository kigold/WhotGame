import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class HeaderInterceptor implements HttpInterceptor {

  constructor(private authService: AuthService) {}
  
  intercept(httpRequest: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (httpRequest.url.includes("connect/token"))
      return next.handle(httpRequest);
    console.log("INtercepting Requests")
    console.log(httpRequest)
    console.log(httpRequest.urlWithParams)
    const jwt = this.authService.getToken()
    return next.handle(httpRequest.clone({ setHeaders: { authorization: `Bearer ${jwt}`  } 
  }));
  }
}
