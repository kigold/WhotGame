import { Component, signal } from '@angular/core';
import { PlayerGameScore } from 'src/app/models/games';
import { GameService } from 'src/app/services/game.service';
import { ActivatedRoute } from '@angular/router';
import { HelperService } from 'src/app/services/helper.service';

@Component({
  selector: 'app-game-score',
  templateUrl: './game-score.component.html',
  styleUrls: ['./game-score.component.css']
})
export class GameScoreComponent {
  playersScores: PlayerGameScore[] = [];
  displayedColumns: string[] = ['position', 'name', 'weight'];
  isLoading = false;
  gameId: number = 0;

  constructor(private gameService: GameService, private activatedroute:ActivatedRoute, private helperService: HelperService){}

  ngOnInit(){
    this.gameId = parseInt(this.activatedroute.snapshot.paramMap.get('id') ?? "0");
    this.isLoading = true;
      this.gameService.getLeaderBoard(this.gameId).subscribe({
        next: (response) => {
          this.playersScores = response.payload;
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          this.gameService.handleError(error)
          this.helperService.toast(error.message)
        }
      })
  }
}
