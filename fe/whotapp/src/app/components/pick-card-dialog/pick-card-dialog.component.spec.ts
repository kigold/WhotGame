import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PickCardDialogComponent } from './pick-card-dialog.component';

describe('PickCardDialogComponent', () => {
  let component: PickCardDialogComponent;
  let fixture: ComponentFixture<PickCardDialogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [PickCardDialogComponent]
    });
    fixture = TestBed.createComponent(PickCardDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
