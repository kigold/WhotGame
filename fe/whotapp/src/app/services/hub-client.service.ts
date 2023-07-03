import { Injectable, signal } from '@angular/core';
import config from '../config';
import { HttpTransportType, HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { BaseService } from './baseService';
import { HttpErrorResponse } from '@angular/common/http';
import { HelperService } from './helper.service';
import { User } from '../models/auth';
import { Game } from '../models/games';
import { Card } from '../models/card';
import { AuthService } from './auth.service';
import { STRING_TYPE } from '@angular/compiler';

@Injectable({
  providedIn: 'root'
})
export class HubClientService implements BaseService {

  constructor(private helperService: HelperService, private authService: AuthService) {
    this.setup();
   }
  conn: HubConnection = {} as HubConnection;
  responseMsg = signal('');
  games = signal(<Game[]>[]);
  cards = signal(<Card[]>[])

  setup(){
    this.conn = new HubConnectionBuilder()
    .configureLogging(LogLevel.Information)
    .withUrl(config.apiBaseUrl + '/gameHub', { accessTokenFactory: () => this.authService.getToken(), skipNegotiation : true, transport: HttpTransportType.WebSockets })
    .withAutomaticReconnect()
    .build();
  }

  startGame(gameId: number){
    this.conn.start().then(() => {
      console.log("SignalR Connected!", { gameId: gameId })
      this.conn.invoke("StartGame", gameId);
    }).catch((err) => {
      this.handleError(err)
    })

    this.conn.on("LoadGame", (msg: string, timeToWaitInSeconds: number) => {
      console.log("Received LoadGame Broadcast", msg, timeToWaitInSeconds);
      //TODO set countdown clock
    })

    this.conn.on("StartGame", (msg: string) => {
      console.log("Received StartGame Broadcast", msg);
      //TODO get Player Cards
    })
  }

  joinGame(gameId: number){
    this.conn.invoke("StartGame", gameId);
  }

  onNewGame(){
    this.conn.on("NewGame", (game: Game) => {
      console.log("New Game Available", game);
      this.games.mutate(values => values.push(game));
    })
    return this.games
  }

  onGameLog(){
    this.conn.on("GameLog", (message: string) => {
      console.log("Received GameLog Message", message);
      //TODO Toast Message
    })
  }

  onCardPlayed(){
    this.conn.on("CardPlayed", (card: Card) => {
      console.log("Received Card Played Message", card);
      //TODO Update the Card on the floor
    })
  }

  onUpdateTurn(){
    this.conn.on("UpdateTurn", (user: User) => {
      console.log("Received UpdateTurn Message", user);
      //TODO handle UpdateTurn, if user == this.User, then enable control else, disable control and Toast the user whos turn it is
    })
  }

  onPickCard(){
    this.conn.on("PickCard", (card: Card) => {
      console.log("Received pick card message", card);
      //TODO receive card and add it to players card
    })
  }

  onEndGame(){
    this.conn.on("EndGame", (user: User) => {
      console.log("Received End Game message", user);
      //TODO show the game winner, get leaderboard and close connections
    })
    return this.games
  }

  receivePlayerMessage(){
    this.conn.on("Player", (user: User) => {
      console.log("Received Player Message", user);
      this.responseMsg.set(user.username);
    })

    return this.responseMsg
  }

  handleError(error: HttpErrorResponse) {
    this.helperService.handleError(error);
  }
}
