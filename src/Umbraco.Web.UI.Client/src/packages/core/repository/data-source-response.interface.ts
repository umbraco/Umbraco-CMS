import type { UmbApiError, UmbCancelError, UmbError } from '@umbraco-cms/backoffice/resources';

export interface UmbDataSourceResponse<T = unknown> extends UmbDataSourceErrorResponse {
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
