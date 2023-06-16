import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import config from './../config';
import { Observable, throwError, lastValueFrom, of, from} from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { LoginRequest, LoginResponseModel, SignupRequest, SignupResponse, User } from '../models/auth';
import JWT from 'jwt-decode';
import { ResponseModel } from '../models/response';
import { HelperService } from './helper.service';
import { BaseService } from './baseService';

@Injectable({
	providedIn: 'root'
})

export class AuthService implements BaseService {

	private SERVER_URL = config.apiBaseUrl;
	constructor(private httpClient: HttpClient, private helperService: HelperService) { }

	login(payload: LoginRequest){
		const requestOptions = {
			headers: {
			  'Content-Type': 'application/x-www-form-urlencoded',
			},
		  };

		const formPayload = new URLSearchParams();
		formPayload.append('grant_type', 'password');
		formPayload.append('password', payload.password);
		formPayload.append('username', payload.email);

		return this.httpClient.post<LoginResponseModel>(this.SERVER_URL + '/connect/token', formPayload, requestOptions);
	}

	storeAuthInLocalStorage(payload: LoginResponseModel): User{
		console.log(payload)
		const user = this.toUser(JWT(payload.access_token));
		localStorage.setItem('profile', JSON.stringify(user));
		localStorage.setItem('token', payload.access_token);
		localStorage.setItem('refresh_token', payload.refresh_token);
		localStorage.setItem('token_expiry', new Date(new Date().getTime() + ((payload.expires_in/60) * 60000)).toString());
		return user as User;
	}

	singup(payload: SignupRequest){
		const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

		return this.httpClient.post<ResponseModel<SignupResponse>>(this.SERVER_URL + '/api/Authorization/CreateUser', payload, requestOptions);
	}

	private toUser(u:any): User{
		return {
			id: parseInt(u.sub as string),
			name: u.name,
			username: u.username
		}
	}

	getUserProfile () {
		const userString = localStorage.getItem('profile');
		if (userString != undefined)
			return JSON.parse(userString) as User;
		return undefined;
	}

	getTokenAndStoreLocally(){
		let token = localStorage.getItem('token');
		if (!token || this.isTokenExpired())
		{
			this.refreshAccessToken();
      token = localStorage.getItem('token');
		}
		return token;
	}

  getToken() : string{
		return localStorage.getItem('token') as string;
	}

	isTokenExpired() {
		const expiryDate = localStorage.getItem('token_expiry');
		if (!expiryDate)
			return true;

		return (new Date().getTime() > Date.parse(expiryDate));
	}

	refreshAccessTokenAndStoreToken() {
		console.log("Refreshing token");
		const requestOptions = {
			headers: {
			  'Content-Type': 'application/x-www-form-urlencoded',
			},
		};

		const refresh_token = localStorage.getItem('refresh_token');
		const formPayload = new URLSearchParams();
		formPayload.append('grant_type', 'refresh_token');
		formPayload.append('refresh_token', refresh_token as string);

		this.httpClient.post<LoginResponseModel>(this.SERVER_URL + '/connect/token', formPayload, requestOptions)
			.subscribe({
				next: (res) => {
            console.log("refreshed Token", res.access_token)
						this.storeAuthInLocalStorage(res as LoginResponseModel);
					},
				error: (e) => this.helperService.handleError(e)
			});
	}

  refreshAccessToken() {
		const requestOptions = {
			headers: {
			  'Content-Type': 'application/x-www-form-urlencoded',
			},
		};

		const refresh_token = localStorage.getItem('refresh_token');

		const formPayload = new URLSearchParams();
		formPayload.append('grant_type', 'refresh_token');
		formPayload.append('refresh_token', refresh_token as string);

		return this.httpClient.post<LoginResponseModel>(this.SERVER_URL + '/connect/token', formPayload, requestOptions);
	}

	logout() {
		localStorage.removeItem('profile');
		localStorage.removeItem('token');
		localStorage.removeItem('refresh_token');
		localStorage.removeItem('token_expiry');
		window.location.reload();
	}

	handleError(error: HttpErrorResponse) {
	this.helperService.handleError(error);
	}
}
