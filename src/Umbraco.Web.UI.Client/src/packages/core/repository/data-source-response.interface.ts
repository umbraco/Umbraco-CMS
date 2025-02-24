import type { ApiError, CancelError } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbDataSourceResponse<T = unknown> extends UmbDataSourceErrorResponse {
	data?: T;
}

export interface UmbDataSourceErrorResponse {
	// TODO: we should not rely on the ApiError and CancelError types from the backend-api package
	// We need to be able to return a generic error type that can be used in the frontend
	// Example: the clipboard is getting is data from local storage, so it should not use the ApiError type
	/**
	 * The error that occurred when fetching the data.
	 * The {ApiError} and {CancelError} types will change in the future to be a more generic error type.
	 */
	error?: ApiError | CancelError;
}
