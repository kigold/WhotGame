import { Component, EventEmitter, Output } from '@angular/core';
import { SignupRequest } from 'src/app/models/auth';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css']
})
export class SignupComponent {

  public model: SignupRequest = {
    firstname: "",
    lastname: "",
    email : "",
    password: ""
  };

  @Output() signIn = new EventEmitter<SignupRequest>()
  @Output() toggleDialog = new EventEmitter()
  
  signup() {
    this.signIn.emit(this.model);
  }

  toggle() {
    this.toggleDialog.emit();
  }
}
