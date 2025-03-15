import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { PvcAppStore } from '../store/pvc-app.signal.store';

@Component({
  selector: 'pvc-stats',
  imports: [
    CommonModule,
    MatCardModule
  ],
  styleUrl: './pvc-home.component.css',
  template: `
    <div *ngIf="pvcAppStore.wdStats() as stats">
      <div>fullPath: {{stats.fullPath}}</div>
      <div>sizeGB: {{stats.sizeGB}}</div>
      <div>possibleSavings: {{stats.possibleSavings}}</div>
    </div>
  `
})
export class PvcStatsComponent implements OnInit {

  pvcAppStore = inject(PvcAppStore);

  folderName = '';
  constructor() {
  }

  ngOnInit() {
    this.pvcAppStore.getWdStats();
  }

}
