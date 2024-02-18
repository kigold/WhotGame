import { Component, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { EMPTY, defer, empty, expand, mergeAll, mergeMap, of } from 'rxjs';
import { Card } from 'src/app/models/card';
import { GameLog, GameStats, PlayerGameScore } from 'src/app/models/games';
import { Player } from 'src/app/models/player';
import { AuthService } from 'src/app/services/auth.service';
import { GameService } from 'src/app/services/game.service';
import { HelperService } from 'src/app/services/helper.service';
import { HubClientService } from 'src/app/services/hub-client.service';

@Component({
  selector: 'app-game-log',
  templateUrl: './game-log.component.html',
  styleUrls: ['./game-log.component.css']
})
export class GameLogComponent {
  isLoading = false;
  gameId: number = 0;
  gameStats = signal(<GameStats>{});
  gameLogs = signal(<GameLog[]>[]);
  players = signal(<Player[]>[]);
  playerTurn = signal(<Player>{});
  playersScores: PlayerGameScore[] = [];
  lastPlayedCard = signal(<Card>{});
  

  constructor(public dialog: MatDialog, private activatedroute:ActivatedRoute, private gameService: GameService,
    private router: Router, private helperService: HelperService,
    private authService: AuthService,
   private hubClient: HubClientService){}

   ngOnInit(){
    this.gameId = parseInt(this.activatedroute.snapshot.paramMap.get('id') ?? "0");
    this.hubClient.initGameLog(this.gameId);
    this.setGameStats();

    this.onStartGame();
    this.onGameLogs();
    this.onCardPlayed();
    this.onUpdateTurn();

    let firstPageObservable = of(this.gameLogs().length);
    console.log("First Observable", firstPageObservable)

    firstPageObservable.pipe(
      expand(page =>{
        return this.gameService.getGameLogs(this.gameId, page).pipe(
          mergeMap(response => {
            console.log("Get Game Logs Reponse", response)
            this.gameLogs.set(this.gameLogs().concat(response.payload.logs));
            let nextPage = response.payload.skip
            return response.payload.hasMore ? of(nextPage) : EMPTY
          })
        );
      })
    ).subscribe(response =>
      {
        console.log("ReceivedGet Game Logs Reponse", this.gameLogs())
      }
    )
  }

  setGameStats(){
    console.log("SETTING GAME STATS FOR GAMEID: ", this.gameId)
    this.gameService.getGameStats(this.gameId).subscribe({
      next: (response) => {
        console.log(response.payload);
        this.gameStats.set((response.payload));
        if (this.gameStats() != undefined && this.gameStats().status == "Started")
        {
          this.isLoading = false;
          this.players.set(response.payload.players);
          this.playerTurn.set(response.payload.players.filter((x: { id: any; }) => x.id == response.payload.currentPlayerId)[0]);
          //Update Players array to reflect when the is a Reverse of the flow
          if (response.payload.isTurnReversed)
            this.players.mutate(x => x.reverse());//TODO Test this
          this.lastPlayedCard.set(response.payload.lastPlayedCard);
        }
        else if (this.gameStats() != undefined && this.gameStats().status == "Ended"){
          this.setLeaderBoard();
        }
      },
      error: (error) => this.gameService.handleError(error)
    })
  }

  setLeaderBoard(){
    this.isLoading = true;
    console.log("SETTING LeaderBoard for GAMEID: ", this.gameId)
    this.gameService.getLeaderBoard(this.gameId).subscribe({
      next: (response) => {
        console.log("Leaderboard Response", response.payload);
        this.playersScores= response.payload;
        this.isLoading = false;
      },
      error: (error) => this.gameService.handleError(error)
    })
 }

 onStartGame(){
  const subscriber = this.hubClient.onStartGame();
  subscriber.subscribe((gameId) => {
    console.log("Starting Game: ", this.gameId, gameId)
    this.isLoading = false;
    //Get Card
    this.setGameStats();
  });
}

  onGameLogs(){
    const subscriber = this.hubClient.onGameLog();
    subscriber.subscribe((log) => {
      console.log("Received Game Log: ", log);
      this.gameLogs.set(this.gameLogs().concat(log));
    });
  }

  onCardPlayed(){
    const subscriber = this.hubClient.onCardPlayed();
    subscriber.subscribe((card) => {
      console.log("Card Played: ", card);
      this.lastPlayedCard.set(card);
    });
  }

  onUpdateTurn(){
    const subscriber = this.hubClient.onUpdateTurn();
    subscriber.subscribe((user) => {
      console.log("Update Turn: ", user);
      this.playerTurn.set({ id: user.id, avatar: user.avatar, name: user.name, status: ""})
    });
  }

}
