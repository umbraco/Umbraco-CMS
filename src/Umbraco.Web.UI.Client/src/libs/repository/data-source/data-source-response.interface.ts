import type { ApiError, CancelError } from 'src/libs/backend-api';

export interface DataSourceResponse<T = undefined> extends UmbDataSourceErrorResponse {
	data?: T;
}

export interface UmbDataSourceErrorResponse {
	error?: ApiError | CancelError;
}
