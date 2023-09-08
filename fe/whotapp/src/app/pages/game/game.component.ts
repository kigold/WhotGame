import { Component, signal } from '@angular/core';
import { Game } from 'src/app/models/games';
import { GameService } from 'src/app/services/game.service';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { HubClientService } from 'src/app/services/hub-client.service';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { Card } from 'src/app/models/card';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent {
  gameId: string = '';
  games = signal(<Game[]>[]);
  game = signal(<Game>{});
  message = signal('Teststr');
  cards = signal(<Card[]>[]);
  card = signal(<Card>{})

  constructor(private activatedroute:ActivatedRoute, private gameService: GameService,
     private hubClient: HubClientService){}

  ngOnInit(){
    this.gameId = this.activatedroute.snapshot.paramMap.get('id') ?? "";

    this.startGame();
  }

  startGame(){
    this.hubClient.connect();
    //this.hubClient.startGame(parseInt(this.gameId));
    //let res = {} as Game
    //this.hubClient.receiveMessage(res);
    //this.message.set(res.name);

    this.message = this.hubClient.receivePlayerMessage();
    this.gameService.getCards().subscribe({
      next: (response) => {
        console.log(response.payload);
        this.cards.set(response.payload);
        this.card.set(response.payload[0])
      },
      error: (error) => this.gameService.handleError(error)
    });

  }

  selectedCard(card: Card){
    this.card.set(card);
  }
}
