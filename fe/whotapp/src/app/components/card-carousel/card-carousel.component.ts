import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { Card } from 'src/app/models/card';

@Component({
  selector: 'app-card-carousel',
  templateUrl: './card-carousel.component.html',
  styleUrls: ['./card-carousel.component.css']
})
export class CardCarouselComponent {
  @Input() cards!: Card[];
  @Input() playButtonDisabled: boolean = false;
  @Output() onPlayCard = new EventEmitter<Card>()
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
  upperSliceIndex = this.cardsCount() > this.cardSlideCount ? this.cards!.length : this.cardSlideCount

  ngOnInit(){
    this.setPageIndex()
  }

  ngOnChanges(){
    this.filteredCards = this.cards;
    this.updateCardsSlide();
    if (this.cards.length > 0)
      this.card.set(this.cards[0]);
  }

  keypress(event: any){
    if (event.key == 'n')
      this.nextCard()
    if (event.key == 'p')
      this.prevCard()
    if (event.key == 'Enter')
      this.playCard()
  }

  setPageIndex(){
    this.cardIndex = 0;
    this.lowerSliceIndex = 0;
    this.upperSliceIndex = this.cardSlideCount;
  }

  prevSlide(){
    this.upperSliceIndex -= (this.upperSliceIndex - this.lowerSliceIndex);
    this.lowerSliceIndex -= this.cardSlideCount;

    if(this.lowerSliceIndex < 0)
      this.lowerSliceIndex = 0;
    if(this.upperSliceIndex < 0)
      this.upperSliceIndex = 0;

    this.updateCardsSlide();

  }

  nextSlide(){
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

    this.setButtonsState()
    this.totalFilteredCards.set(this.filteredCards.length);
    this.totalCards.set(this.cards.length);
  }

  setButtonsState(){
    this.nextSlideButtonDisabled = !(this.cardsCount() > this.upperSliceIndex);
    this.prevSlideButtonDisabled = !(this.lowerSliceIndex > 0);
    this.nextButtonDisabled = !(this.cardsCount() > (this.cardIndex + 1));
    this.prevButtonDisabled = (this.cardIndex <= 0);
  }

  selectCard(card: Card){
    this.card.set(card);
    this.syncCardIndexWithSelectedCard()
    this.setButtonsState()
  }

  playCard(){
    this.totalCards.set(this.cards.length);
    this.onPlayCard.emit(this.card());
  }

  syncCardIndexWithSelectedCard(){
    //Sync Selected card - this.card - with cardIndex
    //Source of truth is the selected card this.card
    this.cardIndex = this.filteredCards.findIndex(x => x.id == this.card().id)
  }

  nextCard(){
    if (this.nextButtonDisabled)
      return

    if ((this.cardIndex + 2) > this.upperSliceIndex)//Update PageSlide with cursor. Ensure Cursor is on the pageSlide
      this.nextSlide()

    if (this.cardsCount() > this.cardIndex)
      this.cardIndex++

    this.updateSelectedCard()
  }

  prevCard(){
    if (this.prevButtonDisabled)
      return

    if ((this.cardIndex - 1) < this.lowerSliceIndex)//Update PageSlide with cursor. Ensure Cursor is on the pageSlide
      this.prevSlide()

    if (this.cardIndex > 0)
      this.cardIndex--;

    this.updateSelectedCard()
  }

  updateSelectedCard(){
    this.card.set(this.filteredCards[this.cardIndex])
    this.setButtonsState()
  }

  clearFilter(){
    this.colorFilter = undefined;
    this.shapeFilter = undefined;
    this.specialCardFilter = undefined;
    this.filter();

    //Reset Card Index
    this.cardIndex = 0;
    this.card.set(this.filteredCards[this.cardIndex]);
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

  getShapeFilterOptions(){
    let set = new Set<string | undefined>();
    set.add(undefined)
    const maxItemsCount = 6;

    for(let i = 0; i < this.cards.length && set.size != maxItemsCount; i++){
      set.add(this.cards[i].shape);
    }
    return Array.from(set);
  }

  getColorFilterOptions(){
    let set = new Set<string | undefined>();
    set.add(undefined)
    const maxItemsCount = 5;

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

  cardsCount(){
    if (this.filteredCards == undefined)
      return 0;
    return this.filteredCards.length
  }

  getPageInfo(): string{
    const page = this.getPage();
    const totalPages = Math.ceil(this.totalFilteredCards()/this.cardSlideCount);
    return "" + page + " - " + totalPages;
  }

  getPage(){
    return (this.lowerSliceIndex/this.cardSlideCount) + 1;
  }
}

//TODO Add Card View for Current Card on table, and card view for Selected Card
//TODO Implement Play Card Button
//TODO enable use or keyboard to play card, arrow keys to select and enter to play card
//TODO nice to have some animation and transitions for card
//TODO Add Page or Card Slide, like page 1 - 10

