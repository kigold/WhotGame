import { Component, signal } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { LoginComponent } from 'src/app/components/login/login.component';
import { SignupComponent } from 'src/app/components/signup/signup.component';
import { LoginRequest, LoginResponseModel, SignupRequest, User } from 'src/app/models/auth';
import { AuthService } from 'src/app/services/auth.service';
import { HelperService } from 'src/app/services/helper.service';

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
  loginResponse: LoginResponseModel = <LoginResponseModel>{};

  constructor(public dialog: MatDialog, private authService: AuthService) {
  }

  ngOnInit(){
    if (!this.isLoggedIn())
    {
      this.showLogin = true;
      this.openLoginDialog();
    }
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
