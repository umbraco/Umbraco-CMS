import type { UmbApiError, UmbCancelError, UmbError } from '../resources/umb-error.js';

export type UmbDataSourceResponse<T> = T & UmbDataSourceResponseWithData & UmbDataSourceErrorResponse;

export interface UmbDataSourceResponseWithData {
	/**
	 * The data returned from the data source.
	 */
	data?: unknown;
}

export interface UmbDataSourceErrorResponse {
	/**
	 * The error that occurred when fetching the data.
	 */
	error?: UmbError | UmbApiError | UmbCancelError | Error;
}
