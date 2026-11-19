import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { ConversionHistoryStore } from '../store/conversion-history.signal.store';

@Component({
  selector: 'pvc-lifetime',
  imports: [CommonModule, MatButtonModule, MatIconModule, MatPaginatorModule, MatProgressBarModule, MatTableModule],
  templateUrl: './pvc-lifetime.component.html',
  styleUrl: './pvc-lifetime.component.css',
})
export class PvcLifetimeComponent implements OnInit {
  readonly history = inject(ConversionHistoryStore);
  readonly columns = ['outputFileName', 'convertedSize', 'percentSizeReduction', 'crf'];

  ngOnInit(): void {
    this.history.loadStats();
  }

  changePage(event: PageEvent): void {
    this.history.setPage(event.pageIndex, event.pageSize);
  }

  formatBytes(bytes: number): string {
    const units = ['B', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB'];
    const index = bytes === 0 ? 0 : Math.min(Math.floor(Math.log(Math.abs(bytes)) / Math.log(1024)), units.length - 1);
    return `${(bytes / Math.pow(1024, index)).toLocaleString(undefined, { maximumFractionDigits: 2 })} ${units[index]}`;
  }
}