import {Component, inject} from '@angular/core';
import {CommonModule} from '@angular/common';
import {MatCardModule} from '@angular/material/card';
import {PvcAppStore} from '../store/pvc-app.signal.store';
import {FileStats} from '../nswag/pvc-client';
import {MatTableModule} from '@angular/material/table';

@Component({
  selector: 'pvc-video-list',
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
  ],
  styleUrl: './pvc-home.component.css',
  template: `
    <mat-card class="pvc-thick-card" appearance="outlined">
      <mat-card-header>
        <mat-card-title>H.264 Videos</mat-card-title>
        <mat-card-subtitle>Convert these videos to reduce space</mat-card-subtitle>
      </mat-card-header>
      <mat-card-content>
        <table mat-table [dataSource]="pvcAppStore.videoTable()">
          <!-- Position Column -->
          <ng-container matColumnDef="fileName">
            <th mat-header-cell *matHeaderCellDef> Name </th>
            <td mat-cell *matCellDef="let element"> {{element.fileName}} </td>
          </ng-container>

          <!-- Weight Column -->
          <ng-container matColumnDef="sizeGB">
            <th mat-header-cell *matHeaderCellDef> Size </th>
            <td mat-cell *matCellDef="let element"> {{element.sizeGB}} </td>
          </ng-container>

          <!-- Name Column -->
          <ng-container matColumnDef="h265Size">
            <th mat-header-cell *matHeaderCellDef> Converted Size </th>
            <td mat-cell *matCellDef="let element"> {{element.h265Size}} </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </mat-card-content>
    </mat-card>
  `
})
export class PvcVideoListComponent {
  pvcAppStore = inject(PvcAppStore);

  displayedColumns: string[] = ['fileName', 'sizeGB', 'h265Size'];

  constructor() {
  }

  protected readonly FileStats = FileStats;
}
