import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Card } from 'src/app/models/card';

@Component({
  selector: 'app-card',
  templateUrl: './card.component.html',
  styleUrls: ['./card.component.css']
})
export class CardComponent {
  @Input() card!: Card;
  @Output() selectCard = new EventEmitter<Card>()

  onSelect(){
    console.log("Inside Card Selecting")
    this.selectCard.emit(this.card);
  }
}
