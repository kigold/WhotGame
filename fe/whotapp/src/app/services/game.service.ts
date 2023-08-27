import { Injectable } from '@angular/core';
import config from '../config';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HelperService } from './helper.service';
import { Game } from '../models/games';
import { ResponseModel } from '../models/response';
import { BaseService } from './baseService';
import { Card } from '../models/card';

@Injectable({
  providedIn: 'root'
})
export class GameService implements BaseService {

  private SERVER_URL = config.apiBaseUrl;
  constructor(private httpClient: HttpClient, private helperService: HelperService) { }

  getCards(){
    //TODO Get Cards from GameGrain
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    return this.httpClient.get<ResponseModel<Card[]>>(this.SERVER_URL + '/api/card', requestOptions);
  }

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
