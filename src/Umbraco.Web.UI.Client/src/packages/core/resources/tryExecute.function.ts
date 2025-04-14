import { UmbTryExecuteController } from './try-execute.controller.js';
import type { UmbApiResponse, UmbTryExecuteOptions } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Make a request and handle errors.
 * @param {UmbControllerHost} host The host to use for the request and where notifications will be shown.
 * @param {Promise<T>} promise The promise to execute.
 * @param {UmbTryExecuteOptions} opts Options for the request.
 * @returns {Promise<UmbApiResponse<T>>} A promise that resolves with the response data or rejects with an error.
 * @template T The type of the response data.
 * @example
 * const { data, error } = await tryExecute(this, myPromise, {
 *   abortSignal: myAbortSignal,
 *   disableNotifications: false,
 * });
 * if (!error) {
 *	 console.log('Success:', data);
 * }
 */
export async function tryExecute<T>(
	host: UmbControllerHost,
	promise: Promise<T>,
	opts?: UmbTryExecuteOptions,
): Promise<UmbApiResponse<T>> {
	const controller = new UmbTryExecuteController(host, promise);
	const response = await controller.tryExecute(opts);
	controller.destroy();
	return response;
}
