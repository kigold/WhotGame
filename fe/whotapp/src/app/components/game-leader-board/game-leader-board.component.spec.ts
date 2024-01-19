import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GameLeaderBoardComponent } from './game-leader-board.component';

describe('GameLeaderBoardComponent', () => {
  let component: GameLeaderBoardComponent;
  let fixture: ComponentFixture<GameLeaderBoardComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [GameLeaderBoardComponent]
    });
    fixture = TestBed.createComponent(GameLeaderBoardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
