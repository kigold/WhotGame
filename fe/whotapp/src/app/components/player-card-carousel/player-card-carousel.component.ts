import { Component, Input } from '@angular/core';
import { Player } from 'src/app/models/player';

@Component({
  selector: 'app-player-card-carousel',
  templateUrl: './player-card-carousel.component.html',
  styleUrls: ['./player-card-carousel.component.css']
})
export class PlayerCardCarouselComponent {
  @Input() players!: Player[];
  @Input() playerTurn!: Player;

  ngOnInit(){
  }

  ngOnChanges(){
  }
}
