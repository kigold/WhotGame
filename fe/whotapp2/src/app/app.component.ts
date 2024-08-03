import { Component } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { User } from './models/auth';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'whotapp2';
  profile: User = { 
    name: "Kaycee",
    avatar: "https://google/com/image",
    id: 1,
    username: "kaycee"
  }
}
