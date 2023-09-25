import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TweetFormComponent } from './tweet-form.component';

describe('TweetFormComponent', () => {
  let component: TweetFormComponent;
  let fixture: ComponentFixture<TweetFormComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [TweetFormComponent]
    });
    fixture = TestBed.createComponent(TweetFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
