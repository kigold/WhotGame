import { Component, EventEmitter, Output } from '@angular/core';
import { CardOptionRequest } from 'src/app/models/card';

@Component({
  selector: 'app-joker-options-dialog',
  templateUrl: './joker-options-dialog.component.html',
  styleUrls: ['./joker-options-dialog.component.css']
})
export class JokerOptionsDialogComponent {

  @Output() playCard = new EventEmitter<CardOptionRequest>()

  ShapeOptions: string[] = [
    'Circle', 'Cross', 'Square', 'Star', 'Triangle',
  ]

  ColorOptions: string[] = [
    'Blue', 'Green', 'Red', 'Yellow'
  ]

  shapeSelection: string = '';
  colorSelection: string = '';

  emitPlayCard(){
    this.playCard.emit({color: this.colorSelection, shape: this.shapeSelection})
  }

}
