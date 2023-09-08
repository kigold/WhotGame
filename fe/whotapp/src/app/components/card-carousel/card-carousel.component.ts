import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { Card } from 'src/app/models/card';

@Component({
  selector: 'app-card-carousel',
  templateUrl: './card-carousel.component.html',
  styleUrls: ['./card-carousel.component.css']
})
export class CardCarouselComponent {
  @Input() cards!: Card[];
  @Output() onSelectCard = new EventEmitter<Card>()
  cardSlideCount = 6;
  cardsInSlide = signal(<Card[]>[]);
  filteredCards:Card[] = [];
  totalCards = signal(<number>0);
  totalFilteredCards = signal(<number>0);
  shapeFilter: string | undefined = undefined;
  colorFilter: string | undefined = undefined;
  specialCardFilter: boolean | undefined = undefined;
  //card = signal(<Card>{});
  prevButtonDisabled: boolean = false;
  nextButtonDisabled: boolean = false;
  lowerSliceIndex = 0;
  upperSliceIndex = this.cardsCount() > 6 ? this.cards!.length : 6

  ngOnChanges(){
    //console.log("OnChanges", this.card())
    console.log("CARDS", this.cards)
    //this.card.set(this.cards[0]);
    this.filteredCards = this.cards;
    this.updateCardsSlide();
  }

  onPrevButton(){
    console.log("PREV")
    this.upperSliceIndex -= (this.upperSliceIndex - this.lowerSliceIndex);
    this.lowerSliceIndex -= this.cardSlideCount;

    if(this.lowerSliceIndex < 0)
      this.lowerSliceIndex = 0;
    if(this.upperSliceIndex < 0)
      this.upperSliceIndex = 0;

    this.updateCardsSlide();

  }

  onNextButton(){
    console.log("NEXT", this.getSpecialCardOptions())
    console.log("Nextting", this.lowerSliceIndex, this.upperSliceIndex)
    this.lowerSliceIndex += this.cardSlideCount;
    this.upperSliceIndex += this.cardSlideCount

    if(this.lowerSliceIndex > this.cardsCount())
      this.lowerSliceIndex = this.cardsCount();
    if(this.upperSliceIndex > this.cardsCount())
      this.upperSliceIndex = this.cardsCount();

      this.updateCardsSlide();
  }

  updateCardsSlide(){
    this.cardsInSlide.set(this.filteredCards.slice(this.lowerSliceIndex, this.upperSliceIndex));
    this.nextButtonDisabled = !(this.cardsCount() > this.upperSliceIndex);
    this.prevButtonDisabled = !(this.lowerSliceIndex > 0);
    this.totalFilteredCards.set(this.filteredCards.length);
    this.totalCards.set(this.cards.length);
  }

  cardsCount(){
    if (this.filteredCards == undefined)
      return 0;
    return this.filteredCards.length
  }

  selectCard(card: Card){
    console.log("Selected Card", card)
    this.onSelectCard.emit(card);
  }

  clearFilter(){
    this.colorFilter = undefined;
    this.shapeFilter = undefined;
    this.specialCardFilter = undefined;
    this.filter();
  }

  filter(){
    this.filteredCards = this.cards;

    if (this.colorFilter != undefined){
      this.filteredCards = this.filteredCards.filter(x => x.color == this.colorFilter)
    }

    if (this.shapeFilter != undefined){
      this.filteredCards = this.filteredCards.filter(x => x.shape == this.shapeFilter)
    }

    if (this.specialCardFilter != undefined){
      this.filteredCards = this.filteredCards.filter(x => x.isSpecial == this.specialCardFilter)
    }

    this.updateCardsSlide();
  }

  getShapeFilterOptions(){
    let set = new Set<string | undefined>();
    set.add(undefined)
    const maxItemsCount = 5;

    for(let i = 0; i < this.cards.length && set.size != maxItemsCount; i++){
      set.add(this.cards[i].shape);
    }
    return Array.from(set);
  }

  getColorFilterOptions(){
    let set = new Set<string | undefined>();
    set.add(undefined)
    const maxItemsCount = 6;

    for(let i = 0; i < this.cards.length && set.size != maxItemsCount; i++){
      set.add(this.cards[i].color);
    }
    return Array.from(set);
  }

  getSpecialCardOptions(){
    let set = new Set<boolean | undefined>();
    set.add(undefined)
    const maxItemsCount = 3;

    for(let i = 0; i < this.cards.length && set.size != maxItemsCount; i++){
      set.add(this.cards[i].isSpecial);
    }
    return Array.from(set);
  }
}

//TODO Add Card View for Current Card on table, and card view for Selected Card
//TODO Implement Play Card Button
//TODO enable use or keyboard to play card, arrow keys to select and enter to play card

