import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import {MatListModule} from '@angular/material/list';
import {MatBadgeModule} from '@angular/material/badge';
import {MatButtonModule} from '@angular/material/button';
import {MatButtonToggleModule} from '@angular/material/button-toggle';
import {MatCardModule} from '@angular/material/card';
import {MatChipsModule} from '@angular/material/chips';
import {MatDialogModule} from '@angular/material/dialog';
import {FormsModule} from '@angular/forms';
import {MatInputModule} from '@angular/material/input';
import {MatIconModule} from '@angular/material/icon';
import {MatProgressBarModule} from '@angular/material/progress-bar';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatSelectModule} from '@angular/material/select';
import {MatToolbarModule} from '@angular/material/toolbar';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HomeComponent } from './pages/home/home.component';
import { GameComponent } from './pages/game/game.component';
import { LoginComponent } from './components/login/login.component';
import { SignupComponent } from './components/signup/signup.component';
import { AuthService } from './services/auth.service';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { HeaderInterceptor } from './interceptors/header.interceptor';
import { PageNotFoundComponent } from './pages/page-not-found/page-not-found.component';
import { CardComponent } from './components/card/card.component';
import { CardCarouselComponent } from './components/card-carousel/card-carousel.component';
import { MatGridListModule } from '@angular/material/grid-list';
import { PlayerCardComponent } from './components/player-card/player-card.component';
import { PlayerCardCarouselComponent } from './components/player-card-carousel/player-card-carousel.component';
import { GameLogsComponent } from './components/game-logs/game-logs.component';
import { PickCardDialogComponent } from './components/pick-card-dialog/pick-card-dialog.component';
import { JokerOptionsDialogComponent } from './components/joker-options-dialog/joker-options-dialog.component';
import { GameLeaderBoardComponent } from './components/game-leader-board/game-leader-board.component';
import { GameLogComponent } from './pages/game-log/game-log.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    GameComponent,
    LoginComponent,
    SignupComponent,
    PageNotFoundComponent,
    CardComponent,
    CardCarouselComponent,
    PlayerCardComponent,
    PlayerCardCarouselComponent,
    GameLogsComponent,
    PickCardDialogComponent,
    JokerOptionsDialogComponent,
    GameLeaderBoardComponent,
    GameLogComponent
  ],
  imports: [
    AppRoutingModule,
    BrowserModule,
    HttpClientModule,
    BrowserAnimationsModule,
    BrowserAnimationsModule,
    FormsModule,
    MatBadgeModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatChipsModule,
    MatDialogModule,
    MatGridListModule,
    MatInputModule,
    MatIconModule,
    MatListModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
    MatToolbarModule,
  ],
  providers: [
    AuthService,
    {
      provide: HTTP_INTERCEPTORS, useClass: HeaderInterceptor, multi:true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
