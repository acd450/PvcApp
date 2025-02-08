import { Component, inject } from '@angular/core';
import { DriveNode, FileChildrenRequest, FileNode } from '../nswag/pvc-client';
import { FileBrowserStore } from '../store/file-browser.signal.store';
import {
  MatButtonToggle,
  MatButtonToggleChange,
  MatButtonToggleGroup,
  MatButtonToggleModule
} from '@angular/material/button-toggle';
import { MatChip, MatChipSet, MatChipsModule } from '@angular/material/chips';
import { MatIcon } from '@angular/material/icon';
import { NgIf } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'pvc-file-browser',
  imports: [
    MatButtonToggleModule,
    MatChipsModule,
    MatIcon,
    NgIf,
    MatButtonModule,
    MatCardModule
  ],
  templateUrl: './file-browser.component.html',
  styleUrl: './file-browser.component.css'
})
export class FileBrowserComponent {
  fbStore = inject(FileBrowserStore);

  constructor() {
    this.fbStore.getRootDrives();
  }

  changeRootDrive(event: MatButtonToggleChange) {
    console.log('changeRootDrive', event);
    let node = event.value as DriveNode;
    this.fbStore.selectDrive(node);
    let req = new FileChildrenRequest({
      path: node.driveLetter, includeFiles: true, includeDirectories: true
    });
    this.fbStore.getChildrenNodes(req);
  }

  selectNode(node: FileNode) {
    this.fbStore.selectNode(node);
    let req = new FileChildrenRequest({
      path: node.path, includeFiles: true, includeDirectories: true
    });
    this.fbStore.getChildrenNodes(req);
  }

  goUpNode() {
    this.fbStore.goUpNode();

    let path = this.fbStore.fullBrowsingPath();
    let req = new FileChildrenRequest({
      path: path[path.length - 1].path, includeFiles: true, includeDirectories: true
    });
    this.fbStore.getChildrenNodes(req);
  }

  setWorkingDirectory() {
    this.fbStore.setWorkingDirectory();
  }

}
