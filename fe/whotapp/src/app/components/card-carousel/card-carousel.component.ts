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
  card = signal(<Card>{});
  cardIndex = 0;
  prevButtonDisabled: boolean = false;
  nextButtonDisabled: boolean = false;
  prevSlideButtonDisabled: boolean = false;
  nextSlideButtonDisabled: boolean = false;
  lowerSliceIndex = 0;
  upperSliceIndex = this.cardsCount() > 6 ? this.cards!.length : 6

  ngOnInit(){
    this.setPageIndex()
  }

  ngOnChanges(){
    this.filteredCards = this.cards;
    this.updateCardsSlide();
    if (this.cards.length > 0)
      this.card.set(this.cards[0]);
  }

  setPageIndex(){
    this.lowerSliceIndex = 0;
    this.upperSliceIndex = this.cardSlideCount
  }

  prevSlide(){
    console.log("PREV")
    this.upperSliceIndex -= (this.upperSliceIndex - this.lowerSliceIndex);
    this.lowerSliceIndex -= this.cardSlideCount;

    if(this.lowerSliceIndex < 0)
      this.lowerSliceIndex = 0;
    if(this.upperSliceIndex < 0)
      this.upperSliceIndex = 0;

    this.updateCardsSlide();

  }

  nextSlide(){
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
    this.nextSlideButtonDisabled = !(this.cardsCount() > this.upperSliceIndex);
    this.prevSlideButtonDisabled = !(this.lowerSliceIndex > 0);
    this.totalFilteredCards.set(this.filteredCards.length);
    this.totalCards.set(this.cards.length);
  }

  selectCard(card: Card){
    this.card.set(card);
  }

  playCard(){
    this.onSelectCard.emit(this.card());
  }

  syncCardIndexWithSelectedCard(){
    //Sync Selected card - this.card - with cardIndex
    //Source of truth is the selected card this.card
    this.cardIndex = this.cards.findIndex(x => x.id == this.card().id)
  }

  nextCard(){
    if (this.cards[this.cardIndex].id != this.card().id)
      this.syncCardIndexWithSelectedCard()

    if (this.cardsCount() > this.cardIndex)
      this.cardIndex++

    this.updateSelectedCard()
  }

  prevCard(){
    if (this.cards[this.cardIndex].id != this.card().id)
      this.syncCardIndexWithSelectedCard()

    if (this.cardIndex > 0)
      this.cardIndex--;

    this.updateSelectedCard()
  }

  updateSelectedCard(){
    this.card.set(this.cards[this.cardIndex])

    this.nextButtonDisabled = !(this.cardsCount() > this.upperSliceIndex);
    this.prevButtonDisabled = !(this.lowerSliceIndex > 0);
  }

  clearFilter(){
    this.colorFilter = undefined;
    this.shapeFilter = undefined;
    this.specialCardFilter = undefined;
    this.filter();
  }

  filter(){
    this.filteredCards = this.cards;
    //reset paging
    this.setPageIndex()

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

  cardsCount(){
    if (this.filteredCards == undefined)
      return 0;
    return this.filteredCards.length
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
//TODO nice to have some animation and transitions for card

