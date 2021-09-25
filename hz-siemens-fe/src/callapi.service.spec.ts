import { TestBed } from '@angular/core/testing';

import { CallapiService } from './callapi.service';

describe('CallapiService', () => {
  let service: CallapiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CallapiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
