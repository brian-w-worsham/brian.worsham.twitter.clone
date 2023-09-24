import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SuggestedUsersPanelComponent } from './suggested-users-panel.component';

describe('SuggestedUsersPanelComponent', () => {
  let component: SuggestedUsersPanelComponent;
  let fixture: ComponentFixture<SuggestedUsersPanelComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [SuggestedUsersPanelComponent]
    });
    fixture = TestBed.createComponent(SuggestedUsersPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
