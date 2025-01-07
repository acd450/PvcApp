import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PvcSettingsComponent } from './pvc-settings.component';

describe('PvcSettingsComponent', () => {
  let component: PvcSettingsComponent;
  let fixture: ComponentFixture<PvcSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PvcSettingsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PvcSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
