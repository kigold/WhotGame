import { Injectable } from '@angular/core';
import config from '../config';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HelperService } from './helper.service';
import { Game } from '../models/games';
import { ResponseModel } from '../models/response';
import { BaseService } from './baseService';
import { Card, GameStats, PlayCardRequest, PlayerGameScore } from '../models/card';

@Injectable({
  providedIn: 'root'
})
export class GameService implements BaseService {

  private SERVER_URL = config.apiBaseUrl;
  constructor(private httpClient: HttpClient, private helperService: HelperService) { }

  getCards(gameId: number){
    //TODO Get Cards from GameGrain
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    return this.httpClient.get<ResponseModel<Card[]>>(this.SERVER_URL + '/api/game/getgamecard/' + gameId, requestOptions);
  }

  getGames(){
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    return this.httpClient.get<ResponseModel<Game[]>>(this.SERVER_URL + '/api/game/getgames', requestOptions);
  }

  getMyActiveGame(){
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    return this.httpClient.get<ResponseModel<Game>>(this.SERVER_URL + '/api/game/getmyactivegame', requestOptions);
  }

  getAvailableGame(){
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    return this.httpClient.get<ResponseModel<Game>>(this.SERVER_URL + '/api/game/getavailablegame', requestOptions);
  }

  getGameStats(gameId: number){
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    return this.httpClient.get<ResponseModel<GameStats>>(this.SERVER_URL + `/api/game/getgamestats/${gameId}`, requestOptions);
  }

  getLeaderBoard(gameId: number){
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    return this.httpClient.get<ResponseModel<PlayerGameScore[]>>(this.SERVER_URL + `/api/game/getGameLeaderboard/${gameId}`, requestOptions);
  }

  joinGame(){
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    return this.httpClient.post<ResponseModel<Game>>(this.SERVER_URL + '/api/game/joingame', {}, requestOptions);
  }

  playCard(card: Card, gameId: number){
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    const request: PlayCardRequest = {
      gameId : gameId,
      cardId: card.id,
      cardColor: card.color,
      cardShape: card.shape
    }

    return this.httpClient.post<ResponseModel<PlayCardRequest>>(this.SERVER_URL + '/api/game/playcard', request, requestOptions);
  }

  pickCard(gameId: number){
    const requestOptions = {
			headers: {
			  'Content-Type': 'application/json',
			},
		};

    return this.httpClient.post<ResponseModel<Card[]>>(this.SERVER_URL + `/api/game/pickcards/${gameId}`,{}, requestOptions);
  }

  handleError(error: HttpErrorResponse) {
    this.helperService.handleError(error);
  }
}
