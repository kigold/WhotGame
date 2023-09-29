import { Component, signal } from '@angular/core';
import { Game } from 'src/app/models/games';
import { GameService } from 'src/app/services/game.service';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { HubClientService } from 'src/app/services/hub-client.service';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { Card } from 'src/app/models/card';
import { Player } from 'src/app/models/player';
import { HelperService } from 'src/app/services/helper.service';

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
  players = signal(<Player[]>[]);

  constructor(private activatedroute:ActivatedRoute, private gameService: GameService,
     private hubClient: HubClientService){}

  ngOnInit(){
    this.gameId = this.activatedroute.snapshot.paramMap.get('id') ?? "";

    this.startGame();
    //TODO Get Players from API
    this.players.set([
      {id: 1, name: "Kaycee", status: "active", avatar: "avatar-1.png"},
      {id: 2, name: "Gift", status: "active", avatar: "avatar-2.png"},
      {id: 3, name: "Jesse", status: "active", avatar: "avatar-3.png"},
      {id: 4, name: "Cele", status: "active", avatar: "avatar-4.png"},
    ])
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

//TODO Add Waiting SPinner stuff to the Game page, when waiting for players to join, if no player joins and we recevie the Abort Message from the Hub then remove spinner
//and go back to home page and show error message that no one joined  the game
