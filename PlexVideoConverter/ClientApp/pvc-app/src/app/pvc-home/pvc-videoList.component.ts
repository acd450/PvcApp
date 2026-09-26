import {Component, effect, inject, Input, OnInit, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {MatCardModule} from '@angular/material/card';
import {PvcAppStore} from '../store/pvc-app.signal.store';
import {FileStats, FolderStats} from '../nswag/pvc-client';
import {MatTableModule} from '@angular/material/table';
import {PvcConversionClientService} from '../service/pvc-conversion-client.service';
import {MatButtonModule, MatIconButton} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';

@Component({
  selector: 'pvc-video-list',
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  styleUrl: './pvc-home.component.css',
  template: `
    <mat-card class="pvc-thick-card" appearance="outlined">
      <mat-card-header>
        <mat-card-title>{{title}}</mat-card-title>
        <mat-card-subtitle>{{description}}</mat-card-subtitle>
      </mat-card-header>
      <mat-card-content>
        <div class="pvc-table-container" [class.pvc-scrollable]="scrollable">
        <table mat-table [dataSource]="videoTable">
          <!-- Position Column -->
          <ng-container matColumnDef="fileName">
            <th mat-header-cell *matHeaderCellDef> Name</th>
            <td mat-cell *matCellDef="let element"> {{ element.fileName }}</td>
          </ng-container>

          <!-- Weight Column -->
          <ng-container matColumnDef="sizeGB">
            <th mat-header-cell *matHeaderCellDef> Size</th>
            <td mat-cell *matCellDef="let element"> {{ element.sizeGB }}</td>
          </ng-container>

          <!-- Name Column -->
          <ng-container matColumnDef="h265Size">
            <th mat-header-cell *matHeaderCellDef> {{h265ColumnHeader}}</th>
            <td mat-cell *matCellDef="let element"> {{ element.h265Size }}</td>
          </ng-container>

          <ng-container matColumnDef="enqueue">
            <th mat-header-cell *matHeaderCellDef> Enqueue </th>
            <td mat-cell *matCellDef="let element">
                @if (isEnqueued(element)) {
                  <mat-spinner diameter="24"></mat-spinner>
                } @else {
                  <button mat-icon-button class="enqueue-button" (click)="enqueueVideo(element)">
                    <mat-icon>add</mat-icon>
                  </button>
                }
            </td>
          </ng-container>

          <ng-container matColumnDef="dequeue">
            <th mat-header-cell *matHeaderCellDef> Dequeue </th>
            <td mat-cell *matCellDef="let element">
                @if (!element.isActive) {
                  <button mat-icon-button class="dequeue-button" color="warn" (click)="dequeueVideo(element)">
                    <mat-icon>delete</mat-icon>
                  </button>
                }
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
        </div>
      </mat-card-content>
    </mat-card>
  `
})
export class PvcVideoListComponent implements OnInit {
  pvcAppStore = inject(PvcAppStore);

  displayedColumns: string[] = ['fileName', 'sizeGB', 'h265Size'];

  @Input() videoTable: any;
  @Input() showEnqueue: boolean = false;
  @Input() showDequeue: boolean = false;
  @Input() scrollable: boolean = false;
  @Input() title = "Unnamed Video List";
  @Input() description = "Unnamed Video List description";
  @Input() h265ColumnHeader = "Converted Size";

  enqueuedFileNames = signal(new Set<string>());

  constructor(public pvcClientService: PvcConversionClientService) {
    // Once the server confirms a file is queued/active, drop our optimistic flag so a
    // later dequeue (which removes it from queuedOrActiveFileNames) makes it enqueuable again
    effect(() => {
      const confirmed = this.pvcClientService.queuedOrActiveFileNames();
      this.enqueuedFileNames.update(names => {
        const updated = new Set(names);
        let changed = false;
        updated.forEach(name => {
          if (confirmed.has(name)) {
            updated.delete(name);
            changed = true;
          }
        });
        return changed ? updated : names;
      });
    });
  }

  ngOnInit() {
    if (this.showEnqueue) {
      this.displayedColumns.push("enqueue");
    }
    if (this.showDequeue) {
      this.displayedColumns.push("dequeue");
    }
  }

  isEnqueued(element: any): boolean {
    return this.enqueuedFileNames().has(element.fileName)
      || this.pvcClientService.queuedOrActiveFileNames().has(element.fileName);
  }

  enqueueVideo(element: any) {
    const stat: FileStats = element.originalData ?? element;

    this.enqueuedFileNames.update(names => new Set(names).add(element.fileName));

    this.pvcClientService.enqueueVideoFile(stat).catch(() => {
      // Revert the loading state if the enqueue request failed to send
      this.enqueuedFileNames.update(names => {
        const updated = new Set(names);
        updated.delete(element.fileName);
        return updated;
      });
    });
  }

  dequeueVideo(element: any) {
    this.pvcClientService.dequeueVideoFile(element.fileName)
      .catch(err => console.error('Error dequeuing video:', err));
  }

  protected readonly FileStats = FileStats;
}

