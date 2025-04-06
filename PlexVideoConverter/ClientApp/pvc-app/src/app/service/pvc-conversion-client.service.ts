import {Injectable} from '@angular/core';
import {HubConnection, HubConnectionBuilder} from '@microsoft/signalr';
import {FileNode, QueueStatusArgs} from '../nswag/pvc-client';

@Injectable({ providedIn: 'root' })
export class PvcConversionClientService {

  private _pvcHubConnection : HubConnection | undefined;

  constructor() {
    this.setupHubConnection()
  }

  setupHubConnection() {
    this._pvcHubConnection = new HubConnectionBuilder()
      .withUrl('/pvcConversionHub')
      .build();

    this._pvcHubConnection.on('QueueStatus', (data: QueueStatusArgs) => {
      console.log(data);
    });

    this._pvcHubConnection.on('ConversionProgressUpdate', (data) => {
      console.log(data);
    });

    this._pvcHubConnection.start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log(err));
  }

  enqueueVideoFile(file: FileNode) {
    this._pvcHubConnection?.invoke('EnqueueConversion', [file]);
  }


}
