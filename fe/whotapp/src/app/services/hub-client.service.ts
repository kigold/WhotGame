import { Injectable, signal } from '@angular/core';
import config from '../config';
import { HttpTransportType, HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr'
import { BaseService } from './baseService';
import { HttpErrorResponse } from '@angular/common/http';
import { HelperService } from './helper.service';
import { User } from '../models/auth';
import { Game } from '../models/games';
import { Card } from '../models/card';
import { AuthService } from './auth.service';
import { STRING_TYPE } from '@angular/compiler';
import { Observable } from 'rxjs';

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

  connect(){
    if (this.conn.state == HubConnectionState.Disconnected){
      this.conn.start().then(() => {
        console.log("SignalR Connected!")
        this.gameCallbacks()

      }).catch((err) => {
        this.handleError(err)
      })
    }
  }

  initGame(gameId: number){
    if (this.conn.state == HubConnectionState.Disconnected){
        this.conn.start().then(() => {
          console.log("SignalR Connected!");
          this.conn.invoke("InitGame", gameId);
        }).catch((err) => {
          this.handleError(err)
        })
    }
    else{
      this.conn.invoke("InitGame", gameId);
    }
  }

  gameCallbacks(){
    this.conn.on("LoadGame", (msg: string, timeToWaitInSeconds: number) => {
      console.log("Received LoadGame Broadcast", msg, timeToWaitInSeconds);
      //TODO set countdown clock
    })

    // this.conn.on("StartGame", (msg: string) => {
    //   console.log("Received StartGame Broadcast", msg);
    //   this.onStartGame();
    //   //TODO get Player Cards
    // })

    this.conn.on("PickCard", (card: Card) => {
      this.onPickCard(card);
    })

    this.conn.on("GameLog", (message: string) => {
      this.onGameLog(message)
    })

    this.conn.on("EndGame", (user: User) => {
      this.onEndGame(user)
    })
  }

  seekGame(){
    this.conn.invoke("SeekGame");
  }

  joinGame(gameId: number){
    this.conn.invoke("StartGame", gameId);
  }

  onAbortGame(): Observable<number>{
    return new Observable<number>(subscriber => {
      this.conn.on("AbortGame", (gameId: number) => {
        console.log("Received AbortGame Broadcast", gameId);
        subscriber.next(gameId);
        subscriber.complete();
      })
    });
  }

  onStartGame(): Observable<number>{
    return new Observable<number>(subscriber => {
      this.conn.on("StartGame", (gameId) => {
        console.log("Game Starting: ", gameId);
        subscriber.next(gameId);
        subscriber.complete();
      })
    });
  }

  onLoadGame(): Observable<string>{
    return new Observable<string>(subscriber => {
      this.conn.on("LoadGame", (msg: string, gameId: number) => {
        console.log("Received Load Game Broadcast", gameId);
        subscriber.next(msg);
        subscriber.complete();
      })
    });
  }

  onCardPlayed(): Observable<Card>{
    return new Observable<Card>(subscriber => {
      this.conn.on("CardPlayed", (card: Card) => {
        console.log("Received Card Played Message", card);
        subscriber.next(card);
      })
    });
  }

  onUpdateTurn(): Observable<User>{
    return new Observable<User>(subscriber => {
      this.conn.on("UpdateTurn", (user: User) => {
        console.log("Received UpdateTurn Message", user);
        subscriber.next(user);
      })
    });
  }

  onReceivedCards(): Observable<Card[]>{
    return new Observable<Card[]>(subscriber => {
      this.conn.on("ReceivedCards", (cards: Card[]) => {
        console.log("Received Cards", cards);
        subscriber.next(cards);
        //subscriber.complete();
      })
    });
  }

  onNewGame(){
    this.conn.on("NewGame", (game: Game) => {
      console.log("New Game Available", game);
      this.games.mutate(values => values.push(game));
    })
    return this.games
  }

  onGameLog(message: string){
    console.log("Received GameLog Message", message);
    //TODO Toast Message
  }

  onPickCard(card: Card){
    console.log("Received pick card message", card);
    //TODO receive card and add it to players card
  }

  onEndGame(user: User){
    console.log("Received End Game message", user);
      //TODO show the game winner, get leaderboard and close connections
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

