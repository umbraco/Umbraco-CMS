import type { UmbDataSourceErrorResponse, UmbDataSourceResponse } from './data-source-response.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbPagedModel<T> {
	items: Array<T>;
	total: number;
}

export interface UmbTargetPagedModel<T> extends UmbPagedModel<T> {
	// TODO: v18: make mandatory
	totalAfter?: number;
	totalBefore?: number;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbRepositoryResponse<T> extends UmbDataSourceResponse<T> {}
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbRepositoryErrorResponse extends UmbDataSourceErrorResponse {}

/**
 * Interface for a repository that can return a paged model.
 * @template T - The type of items in the paged model.
 * @template T$ - The type of items returned by the asObservable method, defaults to T. You should only use this if you want to return a different type from the asObservable method.
 */
export interface UmbRepositoryResponseWithAsObservable<T, T$ = T> extends UmbRepositoryResponse<T> {
	asObservable: () => Observable<T$> | undefined;
}

export type * from './data-mapper/mapping/types.js';
