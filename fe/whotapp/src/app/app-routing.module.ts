import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { GameComponent } from './pages/game/game.component';
import { HomeComponent } from './pages/home/home.component';
import { authguardGuard } from './shared/authguard.guard';
import { PageNotFoundComponent } from './pages/page-not-found/page-not-found.component';
import { GameLogComponent } from './pages/game-log/game-log.component';
import { GameScoreComponent } from './pages/game-score/game-score.component';

const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full'},
  { path: 'home', component: HomeComponent },
  { path: 'game/:id', component: GameComponent, canActivate: [authguardGuard] },
  { path: 'gamelog/:id', component: GameLogComponent },
  { path: 'gamescore/:id', component: GameScoreComponent },
  { path: '**', component: PageNotFoundComponent },  // Wildcard route for a 404 page
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
