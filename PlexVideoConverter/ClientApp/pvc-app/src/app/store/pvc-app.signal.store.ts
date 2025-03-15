import { FolderStats, FolderStatsApiClient } from '../nswag/pvc-client';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { inject } from '@angular/core';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { debounceTime, pipe, switchMap } from 'rxjs';
import { tapResponse } from '@ngrx/operators';

export interface PvcAppState {
  workingDirectory: string,
  wdStats: FolderStats,
}

export const initialState: PvcAppState = {
  workingDirectory: '',
  wdStats: new FolderStats(),
}

export const PvcAppStore = signalStore(
  {providedIn: 'root'},
  withState(initialState),
  //withComputed(({workingDirectory}) => ({})),
  withMethods((store,
               fssClient = inject(FolderStatsApiClient)) => ({
    getWorkingDirectory: rxMethod<void>(
      pipe(
        debounceTime(300),
        switchMap(() => {
          return fssClient.workingdirGET().pipe(
            tapResponse({
              next: (resp) => {
                console.log(`Get Working Directory: ${JSON.stringify(resp)}`);
                patchState(store, {workingDirectory: resp.workingDirectory});
              },
              error: console.error
            }))
        }))
    ),
    getWdStats: rxMethod<void>(
      pipe(
        debounceTime(300),
        switchMap(() => {
          return fssClient.stats().pipe(
            tapResponse({
              next: (resp) => {
                console.log(`Get Stats response: ${JSON.stringify(resp)}`);
                patchState(store, {wdStats: resp});
              },
              error: console.error
            })
          )
        })
      )
    ),
  })),
);
