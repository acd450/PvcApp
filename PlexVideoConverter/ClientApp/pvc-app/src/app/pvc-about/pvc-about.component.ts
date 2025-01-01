import { Component } from '@angular/core';
import { MatCardModule} from '@angular/material/card';
import {CommonModule} from '@angular/common';

@Component({
  selector: 'app-pvc-about',
  imports: [
    CommonModule,
    MatCardModule
  ],
  templateUrl: './pvc-about.component.html',
  styleUrl: './pvc-about.component.css'
})
export class PvcAboutComponent {

}
