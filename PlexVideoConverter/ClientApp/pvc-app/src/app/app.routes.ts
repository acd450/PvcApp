import { Routes } from '@angular/router';
import {PvcHomeComponent} from './pvc-home/pvc-home.component';
import {PvcAboutComponent} from './pvc-about/pvc-about.component';
import {PvcSettingsComponent} from './pvc-settings/pvc-settings.component';

export const routes: Routes = [
  { path: '', component: PvcHomeComponent },
  { path: 'list', component: PvcHomeComponent },
  { path: 'settings', component: PvcSettingsComponent },
  { path: 'about', component: PvcAboutComponent },
];
