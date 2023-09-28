import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Card } from 'src/app/models/card';

@Component({
  selector: 'app-card-mini',
  templateUrl: './card-mini.component.html',
  styleUrls: ['./card-mini.component.css']
})
export class CardMiniComponent {
  @Input() card!: Card;
  @Input() selected!: boolean;
  @Input() size: string = "size-regular";
  @Output() selectCard = new EventEmitter<Card>()

  onSelect(){
    this.selectCard.emit(this.card);
  }
}
