import { Component, Input } from '@angular/core';
import { GameLog } from 'src/app/models/games';

@Component({
  selector: 'app-game-logs',
  templateUrl: './game-logs.component.html',
  styleUrls: ['./game-logs.component.css']
})
export class GameLogsComponent {
  @Input() logs!: GameLog[];
}
