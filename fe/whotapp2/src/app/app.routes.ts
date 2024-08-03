import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { GameComponent } from './pages/game/game.component';
import { authguardGuard } from './utils/authguard.guard';

export const routes: Routes = [
    { path: 'home', component: HomeComponent },
    { path: 'game/:id', component: GameComponent},  
    // { path: 'game/:id', component: GameComponent, canActivate: [authguardGuard] },   
    { path: '', redirectTo: '/home', pathMatch: 'full'},
];
