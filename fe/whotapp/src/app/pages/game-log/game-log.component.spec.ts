import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GameLogComponent } from './game-log.component';

describe('GameLogComponent', () => {
  let component: GameLogComponent;
  let fixture: ComponentFixture<GameLogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [GameLogComponent]
    });
    fixture = TestBed.createComponent(GameLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
