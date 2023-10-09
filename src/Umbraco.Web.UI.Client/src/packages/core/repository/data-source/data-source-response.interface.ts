import type { ApiError, CancelError } from '@umbraco-cms/backoffice/backend-api';

export interface DataSourceResponse<T = unknown> extends UmbDataSourceErrorResponse {
	data?: T;
}

export interface UmbDataSourceErrorResponse {
	error?: ApiError | CancelError;
}
