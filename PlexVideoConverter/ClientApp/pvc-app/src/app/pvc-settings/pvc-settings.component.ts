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
  reportPercentProgressFrontendControl = new FormControl(1);
  reportPercentProgressLoggingControl = new FormControl(20);
  ffmpegSettingsLocationControl = new FormControl("");

  constructor(private pvcSettingsApi: PvcSettingsApiClient) {
    this.pvcSettingsApi.settingsGET()
        .subscribe(settings => {
          this._ffmpegSettings = settings;
          if (settings.videoQuality) this.videoQualityControl.setValue(settings.videoQuality);
          if (settings.reportPercentProgressFrontend) this.reportPercentProgressFrontendControl.setValue(settings.reportPercentProgressFrontend);
          if (settings.reportPercentProgressLogging) this.reportPercentProgressLoggingControl.setValue(settings.reportPercentProgressLogging);
          if (settings.ffmpegSettingsLocation) this.ffmpegSettingsLocationControl.setValue(settings.ffmpegSettingsLocation);
        });

    this.ffmpegFormGroup = new FormGroup({
      videoQuality: this.videoQualityControl,
      reportPercentProgressFrontend: this.reportPercentProgressFrontendControl,
      reportPercentProgressLogging: this.reportPercentProgressLoggingControl,
      ffmpegSettingsLocation: this.ffmpegSettingsLocationControl,
    })
  }

  updateSettings() {
    let updatedSettings = new FfmpegSettings({
      videoQuality: this.videoQualityControl.value ?? this._ffmpegSettings.videoQuality,
      reportPercentProgressFrontend: this.reportPercentProgressFrontendControl.value ?? this._ffmpegSettings.reportPercentProgressFrontend,
      reportPercentProgressLogging: this.reportPercentProgressLoggingControl.value ?? this._ffmpegSettings.reportPercentProgressLogging,
      ffmpegSettingsLocation: this._ffmpegSettings.ffmpegSettingsLocation
    });

    this.pvcSettingsApi.settingsPOST(updatedSettings)
      .subscribe()
  }

}
