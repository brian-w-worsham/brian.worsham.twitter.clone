import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditProileComponent } from './edit-proile.component';

describe('EditProileComponent', () => {
  let component: EditProileComponent;
  let fixture: ComponentFixture<EditProileComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [EditProileComponent]
    });
    fixture = TestBed.createComponent(EditProileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
