import { Component, signal } from '@angular/core';
import { Game, GameLog } from 'src/app/models/games';
import { GameService } from 'src/app/services/game.service';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { HubClientService } from 'src/app/services/hub-client.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { Card, CardOptionRequest, GameStats, PlayerGameScore } from 'src/app/models/card';
import { Player } from 'src/app/models/player';
import { HelperService } from 'src/app/services/helper.service';
import { AuthService } from 'src/app/services/auth.service';
import { User } from 'src/app/models/auth';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { PickCardDialogComponent } from 'src/app/components/pick-card-dialog/pick-card-dialog.component';
import { JokerOptionsDialogComponent } from 'src/app/components/joker-options-dialog/joker-options-dialog.component';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent {
  gameId: number = 0;
  games = signal(<Game[]>[]);
  game = signal(<Game>{});
  message = signal('Teststr');
  cards = signal(<Card[]>[]);
  players = signal(<Player[]>[]);
  playerTurn = signal(<Player>{});
  profile: User | undefined = undefined;
  gameStats = signal(<GameStats>{});
  playersScores: PlayerGameScore[] = [];
  lastPlayedCard = signal(<Card>{});
  disablePlayButton: boolean = false;
  isLoading = false;
  gameLogs = signal(<GameLog[]>[]);
  pickCardDialogRef: MatDialogRef<PickCardDialogComponent, any> = <MatDialogRef<PickCardDialogComponent, any>>{}
  jokerOptionsDialogRef: MatDialogRef<JokerOptionsDialogComponent, any> = <MatDialogRef<JokerOptionsDialogComponent, any>>{}
  pickedCards: Card[] = [];

  constructor(public dialog: MatDialog, private activatedroute:ActivatedRoute, private gameService: GameService,
      private router: Router, private helperService: HelperService,
      private authService: AuthService,
     private hubClient: HubClientService){}

  ngOnInit(){
    this.gameId = parseInt(this.activatedroute.snapshot.paramMap.get('id') ?? "0");
    console.log("Getting Param GameId", this.gameId)
    this.settingUpGame();
    this.setGameStats();
    this.profile = this.authService.getUserProfile();
    this.gameLogs.set([
      { id: 1, message: "Simple card1", color: "Red" },
      { id: 2, message: "Simple card2", color: "Yellow" },
      { id: 3, message: "Simple card3", color: "Blue" },
      { id: 4, message: "Simple card4", color: "Green" }
    ])

    // this.activatedroute.paramMap.subscribe(params => {
    //   this.gameId = params.get('id') ?? ""
    //   console.log("Getting Param GameId", this.gameId)
    //   this.startGame();
    //   this.setGameStats();
    // })
  }

  settingUpGame(){
    console.log("setting Up Game", this.gameId)
    this.isLoading = true;
    this.hubClient.initGame(this.gameId);
    //this.hubClient.gameCallbacks();
    this.onAbortGame();
    this.onStartGame();
    this.onEndGame();
    this.onLoadGame();
    this.onCardPlayed();
    this.onUpdateTurn();
    this.onReceivedCards();
    this.onGameLogs();
    //let res = {} as Game
    //this.hubClient.receiveMessage(res);
    //this.message.set(res.name);

    this.message = this.hubClient.receivePlayerMessage();
    //this.hubClient.gameCallbacks();
    //this.hubClient


    // this.gameService.getCards(parseInt(this.gameId)).subscribe({
    //   next: (response) => {
    //     console.log(response.payload);
    //     this.cards.set(response.payload);
    //     this.card.set(response.payload[0])
    //   },
    //   error: (error) => this.gameService.handleError(error)
    // });

  }

  onAbortGame(){
    const subscriber = this.hubClient.onAbortGame();
    subscriber.subscribe((gameId) => {
      this.isLoading = false;
      this.router.navigate(["home"]);
      this.helperService.toast("No other player joined game");
    });
  }

  onLoadGame(){
    const subscriber = this.hubClient.onLoadGame();
    subscriber.subscribe((msg) => {
      //TODO Set Timer
      console.log(msg)
    });
  }

  onStartGame(){
    const subscriber = this.hubClient.onStartGame();
    subscriber.subscribe((gameId) => {
      console.log("Starting Game: ", this.gameId, gameId)
      this.isLoading = false;
      //Get Card
      this.setCards();
      this.setGameStats();
    });
  }

  onEndGame(){
    const subscriber = this.hubClient.onEndGame();
    subscriber.subscribe((gameId) => {
      console.log("Ending Game: ", this.gameId, gameId)
      this.isLoading = false;
      //Set Leaderboard
      this.setLeaderBoard();

      this.hubClient.close();
    });
  }

  onCardPlayed(){
    const subscriber = this.hubClient.onCardPlayed();
    subscriber.subscribe((card) => {
      console.log("Card Played: ", card);
      this.lastPlayedCard.set(card);
    });
  }

  onReceivedCards(){
    const subscriber = this.hubClient.onReceivedCards();
    subscriber.subscribe((cards) => {
      console.log("Card Received: ", cards);
      this.cards.set(this.cards().concat(cards));
      this.pickCardDialogRef = this.dialog.open(PickCardDialogComponent, {
        data: { cards: cards, message: "Received Cards - General Market" }
      })
      this.pickCardDialogRef.componentInstance.closeDialog.subscribe((x) => {
             console.log("handling Emiting", x);
             this.pickCardDialogRef.close();
            })
    });
  }

  onGameLogs(){
    const subscriber = this.hubClient.onGameLog();
    subscriber.subscribe((log) => {
      console.log("Received Game Log: ", log);
      this.gameLogs.set(this.gameLogs().concat(log));
    });
  }

  onUpdateTurn(){
    const subscriber = this.hubClient.onUpdateTurn();
    subscriber.subscribe((user) => {
      console.log("Update Turn: ", user);
      this.playerTurn.set({ id: user.id, avatar: user.avatar, name: user.name, status: ""})
      this.disablePlayButton = user.id != this.profile?.id;
      if (user.id != this.profile?.id)
        this.helperService.toast(`It is Player ${user.name}'s turn`);
      else
        console.log("TO Enable Play Button");
      //TODO handle UpdateTurn, if user == this.User, then enable control else, disable control and Toast the user whose turn it is
    });
  }

  setCards(){
    this.gameService.getCards(this.gameId).subscribe({
      next: (response) => {
        console.log("SETTING CARD FOR GAME: ", this.gameId)
        console.log(response.payload);
        this.cards.set(response.payload);
        //this.card.set(response.payload[0])
      },
      error: (error) => this.gameService.handleError(error)
    })
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
          this.setCards();
          this.players.set(response.payload.players);
          this.playerTurn.set(response.payload.players.filter(x => x.id == response.payload.currentPlayerId)[0]);
          this.disablePlayButton = response.payload.currentPlayerId != this.profile?.id
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

  playCard(card: Card){
    console.log(card);
    if (card.name == "joker" && (card.shape === "" || card.color === "")){
      this.jokerOptionsDialogRef = this.dialog.open(JokerOptionsDialogComponent, {
        data: { message: "Select Joker Options" }
      })
      this.jokerOptionsDialogRef.componentInstance.playCard.subscribe((request: CardOptionRequest) => {
        card.color = request.color;
        card.shape = request.shape;
        this.callPlayCard(card);
        this.jokerOptionsDialogRef.close();
      })
    }
    else{
      this.callPlayCard(card);
    }
  }

  callPlayCard(card: Card){
    this.isLoading = true;
    this.gameService.playCard(card, this.gameId).subscribe({
      next: (response) => {
        console.log("Played Card: ", this.gameId, card)
        console.log(response.payload);
        if (!response.hasError){
            console.log("SUCCESSFULLY PLAYED CARD");
            this.cards.set(this.cards().filter(x => x.id != card.id))
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.helperService.toast(error.error.message)//Figure out a better way to handle validation messages that are not error
        this.isLoading = false;
      }
    });
  }

  pickCard(){
    this.isLoading = true;
    console.log("Attempting to pick Market");
    this.gameService.pickCard(this.gameId).subscribe({
      next: (response) => {
        console.log("Picked Cards: ", response.payload)
        if (!response.hasError){
            console.log("SUCCESSFULLY Picked CARD");
            //this.cards.mutate(x => x.push(response.payload[0]))
            this.cards.set(this.cards().concat(response.payload))
            this.pickCardDialogRef = this.dialog.open(PickCardDialogComponent, {
              data: { cards: response.payload, message: "picked card" }
            })
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.helperService.toast(error.error.message)//Figure out a better way to handle validation messages that are not error
        this.isLoading = false;
      }
    });
  }
}

//TODO: Better design LEaderboard screen when game ends
//BUG: handle where first card on table is Joker, either select random color andn shape or dont use joker as starting card
//TODO: What happens when a players last card is an instruction like pick 2 or general market
//TODO: We can add a timer to correspond to the time when the game will begin)
//TODO: When Playing Joker a modal should popup asking for color and shape
//TODO: Joker should not have color or shape of its own, and check logic that Joker does not need any validation and will work on all cards
//BUG: supposed to play a continue after playing general market
//BUG: Card Filter seems to break after playing card, might have to reload cards after playing or receiving card
//BUG: The Modal for Pick Card seems to have 3 levels to it, that is, you have to click 3 times to get it to close
//TODO: Add close button to Received market modal
//TODO: Design Joker Card
