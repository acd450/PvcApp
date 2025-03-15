import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCard, MatCardContent, MatCardHeader, MatCardModule } from '@angular/material/card';
import { FileBrowserComponent } from '../file-browser/file-browser.component';
import { PvcAppStore } from '../store/pvc-app.signal.store';
import { PvcStatsComponent } from './pvc-stats.component';
import { PvcGaugeComponent } from './pvc-gauge.component';
import {PvcVideoListComponent} from './pvc-videoList.component';

@Component({
  selector: 'pvc-home',
  imports: [
    CommonModule,
    MatCardModule,
    MatCard,
    MatCardHeader,
    MatCardContent,
    FileBrowserComponent,
    PvcStatsComponent,
    PvcGaugeComponent,
    PvcVideoListComponent,
  ],
  templateUrl: './pvc-home.component.html',
  styleUrl: './pvc-home.component.css'
})
export class PvcHomeComponent {

  pvcAppStore = inject(PvcAppStore);

  folderName = '';
  constructor() {
  }

  checkNewWorkingDirectory() {
    console.log('checkNewWorkingDirectory()');
    this.pvcAppStore.getWorkingDirectory();
  }
}
