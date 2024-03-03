import type { ApiError, CancelError } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbDataSourceResponse<T = unknown> extends UmbDataSourceErrorResponse {
	data?: T;
}

export interface UmbDataSourceErrorResponse {
	error?: ApiError | CancelError;
}
