import { Component, signal } from '@angular/core';
import { Game } from 'src/app/models/games';
import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent {
  games = signal(<Game[]>[]);

  constructor(private gameService: GameService){}

  ngOnInit(){
    this.gameService.getGames()
    .subscribe({
      next: (res) => {
        this.games.set(res.payload)
        console.log(res.payload)
      },      
      error: (e) => this.gameService.handleError(e)
    })
  }
}
