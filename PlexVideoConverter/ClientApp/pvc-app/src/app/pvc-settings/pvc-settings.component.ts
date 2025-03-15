import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCard, MatCardContent, MatCardHeader, MatCardTitle } from "@angular/material/card";
import { FfmpegSettings, PvcSettingsApiClient } from "../nswag/pvc-client";
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatSlider, MatSliderThumb } from '@angular/material/slider';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatTooltip } from '@angular/material/tooltip';

@Component({
  selector: 'app-pvc-settings',
  imports: [
    CommonModule,
    MatCard,
    MatCardTitle,
    MatCardHeader,
    MatCardContent,
    ReactiveFormsModule,
    MatSlider,
    MatSliderThumb,
    MatButton,
    MatIcon,
    MatTooltip,
  ],
  providers: [
    PvcSettingsApiClient
  ],
  templateUrl: './pvc-settings.component.html',
  styleUrl: './pvc-settings.component.css'
})
export class PvcSettingsComponent {

  _ffmpegSettings: FfmpegSettings = new FfmpegSettings();

  ffmpegFormGroup: FormGroup;

  videoQualityControl = new FormControl(20);
  reportPercentProgressControl = new FormControl(10);
  ffmpegSettingsLocationControl = new FormControl("");

  constructor(private pvcSettingsApi: PvcSettingsApiClient) {
    this.pvcSettingsApi.settingsGET()
        .subscribe(settings => {
          this._ffmpegSettings = settings;
          if (settings.videoQuality) this.videoQualityControl.setValue(settings.videoQuality);
          if (settings.reportPercentProgress) this.reportPercentProgressControl.setValue(settings.reportPercentProgress);
          if (settings.ffmpegSettingsLocation) this.ffmpegSettingsLocationControl.setValue(settings.ffmpegSettingsLocation);
        });

    this.ffmpegFormGroup = new FormGroup({
      videoQuality: this.videoQualityControl,
      reportPercentProgress: this.reportPercentProgressControl,
      ffmpegSettingsLocation: this.ffmpegSettingsLocationControl,
    })
  }

  updateSettings() {
    let updatedSettings = new FfmpegSettings({
      videoQuality: this.videoQualityControl.value ?? this._ffmpegSettings.videoQuality,
      reportPercentProgress: this.reportPercentProgressControl.value ?? this._ffmpegSettings.reportPercentProgress,
      ffmpegSettingsLocation: this._ffmpegSettings.ffmpegSettingsLocation
    });

    this.pvcSettingsApi.settingsPOST(updatedSettings)
      .subscribe()
  }

}
