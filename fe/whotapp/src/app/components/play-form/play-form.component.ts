import { Component, EventEmitter, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { GameRequest } from 'src/app/models/games';

@Component({
  selector: 'app-play-form',
  templateUrl: './play-form.component.html',
  styleUrls: ['./play-form.component.css']
})
export class PlayFormComponent {  constructor(public dialog: MatDialog) {

}

game: GameRequest = { gameMode : 'none'}

@Output() play = new EventEmitter<GameRequest>()

triggerPlay() {
  console.log("Triggering", this.game)
  this.play.emit(this.game);
}

}