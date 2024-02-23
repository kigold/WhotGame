import { Component, EventEmitter, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { SignupComponent } from '../signup/signup.component';
import { LoginRequest } from 'src/app/models/auth';
import {MatIconModule} from '@angular/material/icon';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  constructor(public dialog: MatDialog) {
    //dialog.open(SignupComponent)
  }

  model: LoginRequest = {
    email : "",
    password: ""
  };

  @Output() loggedIn = new EventEmitter<LoginRequest>()
  @Output() toggleDialog = new EventEmitter()
  
  login() {
    this.loggedIn.emit(this.model);
  }

  toggle() {
    this.toggleDialog.emit();
  }

}
