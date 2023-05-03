import type { ApiError, CancelError } from '@umbraco-cms/backoffice/backend-api';

export interface DataSourceResponse<T = undefined> extends UmbDataSourceErrorResponse {
	data?: T;
}

export interface UmbDataSourceErrorResponse {
	error?: ApiError | CancelError;
}
