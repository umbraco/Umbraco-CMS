import type { UmbApiError, UmbCancelError, UmbError } from '../resources/umb-error.js';

export interface UmbDataSourceResponse<T> extends UmbDataSourceErrorResponse {
	/**
	 * The data returned from the data source.
	 */
	data?: T;
}

export interface UmbDataSourceErrorResponse {
	/**
	 * The error that occurred when fetching the data.
	 */
	error?: UmbError | UmbApiError | UmbCancelError | Error;
}
