import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import config from '../config';
import { Observable, throwError, lastValueFrom, of, from} from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { LoginRequest, LoginResponseModel, SignupRequest, SignupResponse, User } from '../models/auth';
import JWT from 'jwt-decode';
import { ResponseModel } from '../models/response';
import { HelperService } from './helper.service';
import { BaseService } from './base.service';

@Injectable({
	providedIn: 'root'
})

export class AuthService implements BaseService {

	private SERVER_URL = config.apiBaseUrl;	
	appname: string = "whotapp";
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
		this.setStoreItem('profile', JSON.stringify(user));
		this.setStoreItem('token', payload.access_token);
		this.setStoreItem('refresh_token', payload.refresh_token);
		this.setStoreItem('token_expiry', new Date(new Date().getTime() + ((payload.expires_in/60) * 60000)).toString());
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

	toUser(u:any): User{
		return {
			id: parseInt(u.sub as string),
			name: u.name,
			username: u.username,
      avatar: u.avatar
		}
	}

	getUserProfile () {
		const userString = this.getStoreItem('profile');
		if (userString != undefined)
			return JSON.parse(userString) as User;
		return undefined;
	}

	getTokenAndStoreLocally(){
		let token = this.getStoreItem('token');
		if (!token || this.isTokenExpired())
		{
			this.refreshAccessToken();
      token = this.getStoreItem('token');
		}
		return token;
	}

    getToken() : string{
		return this.getStoreItem('token') as string;
	}

	isTokenExpired() {
		const expiryDate = this.getStoreItem('token_expiry');
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

		const refresh_token = this.getStoreItem('refresh_token');
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

		const refresh_token = this.getStoreItem('refresh_token');

		const formPayload = new URLSearchParams();
		formPayload.append('grant_type', 'refresh_token');
		formPayload.append('refresh_token', refresh_token as string);

		return this.httpClient.post<LoginResponseModel>(this.SERVER_URL + '/connect/token', formPayload, requestOptions);
	}

	logout() {
		this.removeStoreItem('profile');
		this.removeStoreItem('token');
		this.removeStoreItem('refresh_token');
		this.removeStoreItem('token_expiry');
		window.location.reload();
	}

	getStoreItem(key: string){
		return localStorage.getItem(`${this.appname}-${key}`)
	}

	setStoreItem(key: string, value: string){
		localStorage.setItem(`${this.appname}-${key}`, value);
	}

	removeStoreItem(key: string){
		localStorage.removeItem(`${this.appname}-${key}`);
	}

	handleError(error: HttpErrorResponse) {
	this.helperService.handleError(error);
	}
}
