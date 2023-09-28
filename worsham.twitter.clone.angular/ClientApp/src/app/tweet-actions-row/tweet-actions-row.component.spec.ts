import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TweetActionsRowComponent } from './tweet-actions-row.component';

describe('TweetActionsRowComponent', () => {
  let component: TweetActionsRowComponent;
  let fixture: ComponentFixture<TweetActionsRowComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [TweetActionsRowComponent]
    });
    fixture = TestBed.createComponent(TweetActionsRowComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
