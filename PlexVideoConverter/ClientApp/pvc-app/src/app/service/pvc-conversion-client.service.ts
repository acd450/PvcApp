import {computed, Injectable, signal} from '@angular/core';
import {HubConnection, HubConnectionBuilder} from '@microsoft/signalr';
import {ConversionProcess, FileStats, QueueStatusArgs} from '../nswag/pvc-client';

@Injectable({ providedIn: 'root' })
export class PvcConversionClientService {

  private _pvcHubConnection : HubConnection | undefined;

  private _queueStatus = signal<QueueStatusArgs>(new QueueStatusArgs({
    queuedProcesses: [],
    activeProcesses: [],
    completedProcesses: []
  }));

  /** Combined queued + active conversions, shaped for pvc-video-list's table */
  conversionQueueTable = computed(() => {
    const status = this._queueStatus();
    const active = status.activeProcesses ?? [];
    const queued = status.queuedProcesses ?? [];

    return [...active, ...queued].map(p => ({
      originalData: p,
      fileName: p.inputName,
      sizeGB: p.inputSizeGB ? `${p.inputSizeGB} GB` : '',
      h265Size: active.includes(p) ? `${Math.round(p.progress ?? 0)}%` : 'Queued',
    }));
  });

  constructor() {
    this.setupHubConnection()
  }

  setupHubConnection() {
    this._pvcHubConnection = new HubConnectionBuilder()
      .withUrl('/pvcConversionHub')
      .build();

    this._pvcHubConnection.on('QueueStatus', (data: QueueStatusArgs) => {
      this._queueStatus.set(QueueStatusArgs.fromJS(data));
    });

    this._pvcHubConnection.on('ConversionProgressUpdate', (processId: string, progress: number) => {
      this.updateProgress(processId, progress);
    });

    this._pvcHubConnection.start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log(err));
  }

  enqueueVideoFile(file: FileStats): Promise<void> {
    if (this._pvcHubConnection?.state !== 'Connected') {
      console.error('SignalR connection is not established. Current state:', this._pvcHubConnection?.state);
      return Promise.reject('SignalR connection is not established');
    }

    return this._pvcHubConnection?.invoke('EnqueueConversion', [file])
      .catch(err => {
        console.error('Error invoking EnqueueConversion:', err);
        throw err;
      });
  }

  private updateProgress(processId: string, progress: number) {
    const current = this._queueStatus();
    const withProgress = (list?: ConversionProcess[]) =>
      list?.map(p => p.id === processId ? new ConversionProcess({...p, progress}) : p);

    this._queueStatus.set(new QueueStatusArgs({
      queuedProcesses: withProgress(current.queuedProcesses),
      activeProcesses: withProgress(current.activeProcesses),
      completedProcesses: current.completedProcesses
    }));
  }
}

