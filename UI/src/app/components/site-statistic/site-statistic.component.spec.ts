import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SiteStatisticComponent } from './site-statistic.component';

describe('SiteStatisticComponent', () => {
  let component: SiteStatisticComponent;
  let fixture: ComponentFixture<SiteStatisticComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SiteStatisticComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SiteStatisticComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
