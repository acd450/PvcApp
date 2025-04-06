import {Component, inject, Input, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {MatCardModule} from '@angular/material/card';
import {PvcAppStore} from '../store/pvc-app.signal.store';
import {FileStats, FolderStats} from '../nswag/pvc-client';
import {MatTableModule} from '@angular/material/table';
import {PvcConversionClientService} from '../service/pvc-conversion-client.service';
import {MatButtonModule, MatIconButton} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';

@Component({
  selector: 'pvc-video-list',
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
  ],
  styleUrl: './pvc-home.component.css',
  providers: [PvcConversionClientService],
  template: `
    <mat-card class="pvc-thick-card" appearance="outlined">
      <mat-card-header>
        <mat-card-title>{{title}}</mat-card-title>
        <mat-card-subtitle>{{description}}</mat-card-subtitle>
      </mat-card-header>
      <mat-card-content>
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
            <th mat-header-cell *matHeaderCellDef> Converted Size</th>
            <td mat-cell *matCellDef="let element"> {{ element.h265Size }}</td>
          </ng-container>

          <ng-container matColumnDef="enqueue">
            <th mat-header-cell *matHeaderCellDef> Enqueue </th>
            <td mat-cell *matCellDef="let element">
                <button mat-icon-button (click)="enqueueVideo(element)">
                  <mat-icon>add</mat-icon>
                </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </mat-card-content>
    </mat-card>
  `
})
export class PvcVideoListComponent implements OnInit {
  pvcAppStore = inject(PvcAppStore);

  displayedColumns: string[] = ['fileName', 'sizeGB', 'h265Size'];

  @Input() videoTable: any;
  @Input() showEnqueue: boolean = false;
  @Input() title = "Unnamed Video List";
  @Input() description = "Unnamed Video List description";

  constructor(public pvcClientService: PvcConversionClientService) {
  }

  ngOnInit() {
    if (this.showEnqueue) {
      this.displayedColumns.push("enqueue");
      console.log("Showing Enqueue");
    }
  }

  enqueueVideo(stat: FolderStats) {
    this.pvcClientService.enqueueVideoFile(stat)
  }

  protected readonly FileStats = FileStats;
}
