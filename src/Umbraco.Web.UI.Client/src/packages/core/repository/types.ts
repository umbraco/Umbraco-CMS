import type { UmbDataSourceResponse } from './data-source-response.interface.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbPagedModel<T> {
	total: number;
	items: Array<T>;
}

export interface UmbRepositoryResponse<T> extends UmbDataSourceResponse {}

export interface UmbRepositoryResponseWithAsObservable<T> extends UmbRepositoryResponse<T> {
	asObservable?: () => Observable<T | undefined>;
}
