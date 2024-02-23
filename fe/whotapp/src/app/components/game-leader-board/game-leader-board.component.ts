import { Component, Input } from '@angular/core';
import { PlayerGameScore } from 'src/app/models/games';

@Component({
  selector: 'app-game-leader-board',
  templateUrl: './game-leader-board.component.html',
  styleUrls: ['./game-leader-board.component.css']
})
export class GameLeaderBoardComponent {
  @Input() playerScore!: PlayerGameScore;
  @Input() position!: number;
}
