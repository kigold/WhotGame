import { Injectable } from '@angular/core';
import config from '../config';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HelperService } from './helper.service';
import { Game } from '../models/games';
import { ResponseModel } from '../models/response';
import { BaseService } from './baseService';

@Injectable({
  providedIn: 'root'
})
export class GameService implements BaseService {

  private SERVER_URL = config.apiBaseUrl;
  constructor(private httpClient: HttpClient, private helperService: HelperService) { }

  getGames(){
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    return this.httpClient.get<ResponseModel<Game[]>>(this.SERVER_URL + '/api/game/getgames', requestOptions);
  }

  getActiveGame(){
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    return this.httpClient.get<ResponseModel<Game>>(this.SERVER_URL + '/api/game/getactivegame', requestOptions);
  }

  joinGame(){
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    return this.httpClient.post<ResponseModel<Game>>(this.SERVER_URL + '/api/game/joingame', {}, requestOptions);
  }

  handleError(error: HttpErrorResponse) {
    this.helperService.handleError(error);
  }
}
