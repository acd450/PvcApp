import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PvcAboutComponent } from './pvc-about.component';

describe('PvcAboutComponent', () => {
  let component: PvcAboutComponent;
  let fixture: ComponentFixture<PvcAboutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PvcAboutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PvcAboutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
