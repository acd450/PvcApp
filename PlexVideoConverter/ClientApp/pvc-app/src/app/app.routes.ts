import { Routes } from '@angular/router';
import {PvcHomeComponent} from './pvc-home/pvc-home.component';
import {PvcAboutComponent} from './pvc-about/pvc-about.component';
import {PvcSettingsComponent} from './pvc-settings/pvc-settings.component';
import { importProvidersFrom } from '@angular/core';
import { FileBrowserStore } from './store/file-browser.signal.store';
import { PvcAppStore } from './store/pvc-app.signal.store';

export const routes: Routes = [
  {
    path: '', component: PvcHomeComponent,
    providers: [importProvidersFrom(FileBrowserStore, PvcAppStore)],
  },
  { path: 'list', component: PvcHomeComponent },
  { path: 'settings', component: PvcSettingsComponent },
  { path: 'about', component: PvcAboutComponent },
];
