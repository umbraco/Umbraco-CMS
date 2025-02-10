import type { UmbDataSourceErrorResponse, UmbDataSourceResponse } from './data-source-response.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbPagedModel<T> {
	total: number;
	items: Array<T>;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbRepositoryResponse<T> extends UmbDataSourceResponse<T> {}
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbRepositoryErrorResponse extends UmbDataSourceErrorResponse {}

export interface UmbRepositoryResponseWithAsObservable<T> extends UmbRepositoryResponse<T> {
	asObservable: () => Observable<T | undefined>;
}

export type * from './data-mapper/types.js';
