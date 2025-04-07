import type { UmbApiError, UmbCancelError, UmbError } from '../resources/umb-error.js';

export interface UmbDataSourceResponse<T = unknown> extends UmbDataSourceErrorResponse {
	data?: T;
}

export interface UmbDataSourceErrorResponse {
	/**
	 * The error that occurred when fetching the data.
	 */
	error?: UmbError | UmbApiError | UmbCancelError | Error;
}
