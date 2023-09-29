import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlayerCardCarouselComponent } from './player-card-carousel.component';

describe('PlayerCardCarouselComponent', () => {
  let component: PlayerCardCarouselComponent;
  let fixture: ComponentFixture<PlayerCardCarouselComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [PlayerCardCarouselComponent]
    });
    fixture = TestBed.createComponent(PlayerCardCarouselComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
