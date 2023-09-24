import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TweetsfeedComponent } from './tweetsfeed.component';

describe('TweetsfeedComponent', () => {
  let component: TweetsfeedComponent;
  let fixture: ComponentFixture<TweetsfeedComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [TweetsfeedComponent]
    });
    fixture = TestBed.createComponent(TweetsfeedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
