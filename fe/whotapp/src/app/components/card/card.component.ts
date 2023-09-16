import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { Card } from 'src/app/models/card';

@Component({
  selector: 'app-card',
  templateUrl: './card.component.html',
  styleUrls: ['./card.component.css']
})
export class CardComponent {
  @Input() card!: Card;
  @Input() selected!: boolean;
  @Output() selectCard = new EventEmitter<Card>()

  onSelect(){
    this.selectCard.emit(this.card);
  }
}
