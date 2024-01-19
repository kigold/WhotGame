import { Component, Input, Inject, Output, EventEmitter } from '@angular/core';
import { Card } from 'src/app/models/card';
import {MAT_DIALOG_DATA} from '@angular/material/dialog';

@Component({
  selector: 'app-pick-card-dialog',
  templateUrl: './pick-card-dialog.component.html',
  styleUrls: ['./pick-card-dialog.component.css']
})
export class PickCardDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: { cards: Card[], message: string } ) {
    console.log("DIALGOING CARD", data.cards);
  }
  @Output() closeDialog = new EventEmitter<string>()

  emitCloseDialog(){
    console.log("Emiting")
    this.closeDialog.emit("Emiting Message");
  }
}
