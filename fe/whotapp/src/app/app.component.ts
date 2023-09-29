import { Component } from '@angular/core';
import {MatCardModule} from '@angular/material/card';
import { AuthService } from './services/auth.service';
import { Router } from '@angular/router';
import { Player } from './models/player';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  constructor(private authService: AuthService, private router: Router){
    this.player = {id: 1, name: "", status: "", avatar: ""}
  }
  ngOnInit(){
    //this.player = {id: 1, name: "Kaycee", status: "active", avatar: "avatar-1.png"}
    const user = this.authService.getUserProfile();
    if (user != undefined)
      this.player = {id: user.id, name: user.name, status: "", avatar: user.avatar}
  }
  title = 'whotapp';
  player: Player

  logout(){
    this.authService.logout()
  }
}
