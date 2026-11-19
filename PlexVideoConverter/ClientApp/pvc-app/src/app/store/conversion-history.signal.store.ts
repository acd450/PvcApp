import { computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { tapResponse } from '@ngrx/operators';
import { exhaustMap, filter, mergeMap, pipe, tap } from 'rxjs';
import { API_BASE_URL } from '../nswag/pvc-client';

export interface LifetimeConversionStats {
  totalConversions: number;
  originalSize: number;
  convertedSize: number;
  bytesSaved: number;
  percentSizeReduction: number;
  averageCrf: number;
  totalDurationSeconds: number;
  firstConversion: string | null;
  latestConversion: string | null;
  snapshotId: number;
}

export interface ConversionHistoryEntry {
  conversionId: string;
  originalSize: number;
  convertedSize: number;
  inputFileName: string;
  outputFileName: string;
  inputPath: string;
  outputPath: string;
  dateTimeConverted: string;
  percentSizeReduction: number;
  crf: number;
  videoCodec: string;
  durationSeconds: number;
}

export interface ConversionHistoryPage {
  items: ConversionHistoryEntry[];
  page: number;
  pageSize: number;
  totalCount: number;
}

interface PageRequest {
  page: number;
  pageSize: number;
}

const pageKey = ({ page, pageSize }: PageRequest) => `${page}:${pageSize}`;

export const ConversionHistoryStore = signalStore(
  { providedIn: 'root' },
  withState({
    stats: null as LifetimeConversionStats | null,
    statsLoading: false,
    statsError: '',
    pages: {} as Record<string, ConversionHistoryPage>,
    loadingPages: {} as Record<string, boolean>,
    pageErrors: {} as Record<string, string>,
    pageIndex: 0,
    pageSize: 25,
  }),
  withComputed(({ pages, loadingPages, pageErrors, pageIndex, pageSize }) => ({
    entries: computed(() => pages()[pageKey({ page: pageIndex(), pageSize: pageSize() })]?.items ?? []),
    pageLoading: computed(() => loadingPages()[pageKey({ page: pageIndex(), pageSize: pageSize() })] ?? false),
    pageError: computed(() => pageErrors()[pageKey({ page: pageIndex(), pageSize: pageSize() })] ?? ''),
  })),
  withMethods((store, http = inject(HttpClient), baseUrl = inject(API_BASE_URL, { optional: true }) ?? '') => {
    const loadPage = rxMethod<PageRequest>(pipe(
      filter(request => store.stats() !== null && !store.pages()[pageKey(request)] && !store.loadingPages()[pageKey(request)]),
      tap(request => patchState(store, {
        loadingPages: { ...store.loadingPages(), [pageKey(request)]: true },
        pageErrors: { ...store.pageErrors(), [pageKey(request)]: '' },
      })),
      mergeMap(request => http.get<ConversionHistoryPage>(`${baseUrl}/api/conversions/history`, {
        params: { page: request.page, pageSize: request.pageSize, snapshotId: store.stats()!.snapshotId },
      }).pipe(tapResponse({
        next: response => patchState(store, { pages: { ...store.pages(), [pageKey(request)]: response } }),
        error: () => patchState(store, {
          pageErrors: { ...store.pageErrors(), [pageKey(request)]: 'Conversion history could not be loaded.' },
        }),
        finalize: () => patchState(store, { loadingPages: { ...store.loadingPages(), [pageKey(request)]: false } }),
      }))),
    ));

    return {
      loadStats: rxMethod<void>(pipe(
        filter(() => !store.stats() && !store.statsLoading()),
        tap(() => patchState(store, { statsLoading: true, statsError: '' })),
        exhaustMap(() => http.get<LifetimeConversionStats>(`${baseUrl}/api/conversions/stats`).pipe(tapResponse({
          next: stats => {
            patchState(store, { stats });
            loadPage({ page: store.pageIndex(), pageSize: store.pageSize() });
          },
          error: () => patchState(store, { statsError: 'Lifetime statistics could not be loaded.' }),
          finalize: () => patchState(store, { statsLoading: false }),
        }))),
      )),
      setPage(page: number, pageSize: number): void {
        patchState(store, { pageIndex: page, pageSize });
        loadPage({ page, pageSize });
      },
      retryPage(): void {
        loadPage({ page: store.pageIndex(), pageSize: store.pageSize() });
      },
    };
  }),
);