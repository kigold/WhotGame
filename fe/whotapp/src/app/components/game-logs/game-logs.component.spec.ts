import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GameLogsComponent } from './game-logs.component';

describe('GameLogsComponent', () => {
  let component: GameLogsComponent;
  let fixture: ComponentFixture<GameLogsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [GameLogsComponent]
    });
    fixture = TestBed.createComponent(GameLogsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
