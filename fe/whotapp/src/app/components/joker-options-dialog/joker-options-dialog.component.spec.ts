import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JokerOptionsDialogComponent } from './joker-options-dialog.component';

describe('JokerOptionsDialogComponent', () => {
  let component: JokerOptionsDialogComponent;
  let fixture: ComponentFixture<JokerOptionsDialogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [JokerOptionsDialogComponent]
    });
    fixture = TestBed.createComponent(JokerOptionsDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
