import {Component} from '@angular/core';
import {CommonModule} from '@angular/common';
import { MatCard, MatCardContent, MatCardHeader, MatCardModule } from '@angular/material/card';
import { MatIcon } from '@angular/material/icon';
import { MatMiniFabButton } from '@angular/material/button';

@Component({
  selector: 'pvc-home',
  imports: [
    CommonModule,
    MatCardModule,
    MatCard,
    MatCardHeader,
    MatCardContent,
    MatIcon,
    MatMiniFabButton,
  ],
  templateUrl: './pvc-home.component.html',
  styleUrl: './pvc-home.component.css'
})
export class PvcHomeComponent {

  folderName = '';
  constructor() {
  }

  onFolderSelected(event: Event) {

    const folderName = event;
    console.log(folderName);

    // if (file) {
    //
    //   this.fileName = file.name;
    //
    //   const formData = new FormData();
    //
    //   formData.append("thumbnail", file);
    //
    //   const upload$ = this.http.post("/api/thumbnail-upload", formData);
    //
    //   upload$.subscribe();
    // }
  }
}
