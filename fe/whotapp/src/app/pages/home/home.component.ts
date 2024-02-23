import { Component, signal } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { LoginComponent } from 'src/app/components/login/login.component';
import { SignupComponent } from 'src/app/components/signup/signup.component';
import { LoginRequest, LoginResponseModel, SignupRequest, User } from 'src/app/models/auth';
import { Game, GameRequest } from 'src/app/models/games';
import { AuthService } from 'src/app/services/auth.service';
import { GameService } from 'src/app/services/game.service';
import { HelperService } from 'src/app/services/helper.service';
import {MatListModule} from '@angular/material/list';
import { HubClientService } from 'src/app/services/hub-client.service';
import { Router } from '@angular/router';
import { PlayFormComponent } from 'src/app/components/play-form/play-form.component';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  profile = signal(<User>{});
  showLogin: boolean = false;
  showSignup: boolean = false;
  loginDialogRef: MatDialogRef<LoginComponent, any> = <MatDialogRef<LoginComponent, any>>{}
  signupDialogRef: MatDialogRef<SignupComponent, any> = <MatDialogRef<SignupComponent, any>>{}
  playDialogRef: MatDialogRef<PlayFormComponent, any> = <MatDialogRef<PlayFormComponent, any>>{}
  loginResponse: LoginResponseModel = <LoginResponseModel>{};
  games = signal(<Game[]>[]);

  constructor(public dialog: MatDialog, private authService: AuthService,
    private gameService: GameService, private hubClient: HubClientService,
    private helperService: HelperService,
    private router: Router) {
  }

  ngOnInit(){
    if (!this.isLoggedIn()){
      this.showLogin = true;
      this.openLoginDialog();
    }
    else{
      //We do not need to manually select game, this is now selected automatically if available
      // this.gameService.getGames()
      // .subscribe({
      //   next: (res) => {
      //     this.games.set(res.payload)
      //     console.log(res.payload)
      //   },
      //   error: (e) => this.gameService.handleError(e)
      // })

      this.gameService.getMyActiveGame()
      .subscribe({
        next: (response) => {
          if (!response.hasError){
            console.log("Available game found", response.payload)
            this.router.navigate([`game/${response.payload.id}`])
          }
        },
        error: (e) => {
          console.log("No Active games found.");
          this.helperService.toast("No Active games found., click on 'PLAY' button")
        }
      });

      this.gameService.getGames()
      .subscribe({
        next: (response) => {
          console.log("Get Games", response)
          this.games.set(response.payload)
        },
        error: (e) => {
          console.log("No Active games found.");
          this.helperService.toast("No Active games found., click on 'PLAY' button")
        }
      });

      //Listen for New Games and Add to list
      // this.hubClient.connect();
      // this.games = this.hubClient.onNewGame()
    }
  }

  openPlayDialog(){
    this.playDialogRef = this.dialog.open(PlayFormComponent)
    this.playDialogRef.componentInstance.play.subscribe((request: GameRequest) => this.play(request))
  }

  play(request: GameRequest){
    this.playDialogRef.close();

    this.gameService.joinGame(request)
    .subscribe({
      next: (gameResponse) => {
       console.log("Joining Game: ", gameResponse.payload)
       this.router.navigate([`game/${gameResponse.payload.id}`])
      },
      error: (error) => {this.gameService.handleError(error)}
    })
  }

  isLoggedIn(){
    const user = this.authService.getUserProfile();
    if (user == undefined)
      return false;
    this.profile.mutate(value => {
      value.id = user.id,
      value.name = user.name,
      value.username = user.username
    })
    return true;
  }

  openLoginDialog() {
    this.loginDialogRef = this.dialog.open(LoginComponent)
    this.loginDialogRef.componentInstance.loggedIn.subscribe((request: LoginRequest) => this.onLogin(request))
    this.loginDialogRef.componentInstance.toggleDialog.subscribe(() => this.toggleLoginSingupDialog())
  }

  openSignupDialog() {
    this.signupDialogRef = this.dialog.open(SignupComponent)
    this.signupDialogRef.componentInstance.signIn.subscribe((request: SignupRequest) => this.onSignup(request))
    this.signupDialogRef.componentInstance.toggleDialog.subscribe(() => this.toggleLoginSingupDialog())
  }

  toggleLoginSingupDialog() {
    if (this.showLogin){
      this.showLogin = false;
      this.loginDialogRef.close();
      this.showSignup = true;
      this.openSignupDialog();
    }
    else{
      this.showSignup = false;
      this.signupDialogRef.close();
      this.showLogin = true;
      this.openLoginDialog();
    }
  }

  onLogin(request: LoginRequest){
    this.authService.login(request)
    .subscribe({
      next: (res) => {
        let user = this.authService.storeAuthInLocalStorage(res);
        this.profile.mutate(value => {
          value.id = user.id,
          value.name = user.name,
          value.username = user.username
        });
        this.loginDialogRef.close();
      },
      error: (e) => this.authService.handleError(e)
    })
  }


  onSignup(request: SignupRequest) {
    console.log("Home - Signing Up", request)
    this.authService.singup(request)
    .subscribe({
      next: (res) => {
        this.signupDialogRef.close();
        console.log(res);
      }
    })
  }
}
