import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TweetAndRelatedCommentsComponent } from './tweet-and-related-comments.component';

describe('TweetAndRelatedCommentsComponent', () => {
  let component: TweetAndRelatedCommentsComponent;
  let fixture: ComponentFixture<TweetAndRelatedCommentsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [TweetAndRelatedCommentsComponent]
    });
    fixture = TestBed.createComponent(TweetAndRelatedCommentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
