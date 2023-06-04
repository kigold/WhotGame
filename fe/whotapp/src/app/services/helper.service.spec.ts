import { TestBed } from '@angular/core/testing';

import { HelperService } from './helper.service';

describe('ToastService', () => {
  let service: HelperService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(HelperService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
