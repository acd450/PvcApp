import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import {
  DriveNode,
  FileBrowserApiClient,
  FileChildrenRequest,
  FileNode,
  FolderStatsApiClient
} from '../nswag/pvc-client';
import { computed, inject } from '@angular/core';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { debounceTime, pipe, switchMap } from 'rxjs';
import { tapResponse } from '@ngrx/operators';

export interface FileBrowserState {
  fullBrowsingPath: FileNode[],
  childNodes: FileNode[],
  driveList: DriveNode[]
}

export const initialState: FileBrowserState = {
  fullBrowsingPath: [],
  childNodes: [],
  driveList: []
}

export const FileBrowserStore = signalStore(
  {providedIn: 'root'},
  withState(initialState),
  withComputed(({fullBrowsingPath}) => ({
    browsingPathString: computed(() => {
      return fullBrowsingPath().map(fbp => fbp.name).toString().replace(',', '').replaceAll(",",'\\');
    })
  })),
  withMethods((store,
               fbClient = inject(FileBrowserApiClient),
               fssClient = inject(FolderStatsApiClient)) => ({
      getRootDrives: rxMethod<void>(
        pipe(
          debounceTime(300),
          switchMap(() => {
            return fbClient.root().pipe(
              tapResponse({
                next: (drives) => patchState(store, {driveList: drives}),
                error: console.error
              }))
          }))
      ),
      getChildrenNodes: rxMethod<FileChildrenRequest>(
        pipe(
          debounceTime(300),
          switchMap((req) => {
            return fbClient.children(req).pipe(
              tapResponse({
                next: (resp) => patchState(store, {childNodes: resp}),
                error: console.error
              }))
          }))
      ),
      setWorkingDirectory: rxMethod<void>(
        pipe(
          debounceTime(300),
          switchMap(() => {
            return fssClient.workingdirPOST(store.fullBrowsingPath()[store.fullBrowsingPath().length-1].path).pipe(
              tapResponse({
                next: (resp) => console.log("Set Working Directory."),
                error: console.error
              }))
          }))
      ),
      selectDrive: (node: DriveNode) => {
        let fileNode = new FileNode({ path: node.driveLetter, isDirectory: true, name: node.driveLetter, hasChildren: true });
        patchState(store, {fullBrowsingPath: [fileNode]});
      },
      selectNode: (node: FileNode) => {
        patchState(store, (state) => ({fullBrowsingPath: [...state.fullBrowsingPath, node]}));
      },
      goUpNode: () => {
        patchState(store, (state) => {
            let path = [...state.fullBrowsingPath];
            path.pop();
            return {fullBrowsingPath: path};
        });
      }
    })
  )
)
