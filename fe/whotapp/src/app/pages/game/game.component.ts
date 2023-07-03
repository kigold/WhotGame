import { Component, signal } from '@angular/core';
import { Game } from 'src/app/models/games';
import { GameService } from 'src/app/services/game.service';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { HubClientService } from 'src/app/services/hub-client.service';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent {
  gameId: string = '';
  games = signal(<Game[]>[]);
  //games = signal(['']);
  message = signal('Teststr');

  constructor(private activatedroute:ActivatedRoute, private gameService: GameService, private hubClient: HubClientService){}

  ngOnInit(){
    this.gameId = this.activatedroute.snapshot.paramMap.get('id') ?? "";
    // this.gameService.getGames()
    // .subscribe({
    //   next: (res) => {
    //     this.games.set(res.payload)
    //     console.log(res.payload)
    //   },
    //   error: (e) => this.gameService.handleError(e)
    // })

    this.startGame();
  }

  startGame(){
    this.hubClient.startGame(parseInt(this.gameId));
    //let res = {} as Game
    //this.hubClient.receiveMessage(res);
    //this.message.set(res.name);

    this.message = this.hubClient.receivePlayerMessage();
    //this.games = this.hubClient.receiveGameMessage();

  }
}
