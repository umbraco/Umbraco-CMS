import { UmbTryExecuteController } from './try-execute.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * Make a request and notify the user of any errors.
 * This function is a wrapper around the {@link tryExecute} function and will notify the user of any errors.
 * It is useful for making requests where you want to handle errors in a consistent way.
 * @param {UmbControllerHost} host The host to use for the request.
 * @param {Promise<T>} resource The resource to request.
 * @returns {Promise<UmbDataSourceResponse<T>>} A promise that resolves with the response data or rejects with an error.
 * @template T The type of the response data.
 * @deprecated Use the {@link tryExecute} function instead and handle the error in the caller.
 * This function is kept for backwards compatibility and will be removed in a future version.
 */
export async function tryExecuteAndNotify<T>(
	host: UmbControllerHost,
	resource: Promise<T>,
): Promise<UmbDataSourceResponse<T>> {
	new UmbDeprecation({
		deprecated: 'The tryExecuteAndNotify function is deprecated.',
		removeInVersion: '18.0.0',
		solution: 'Use the tryExecute function with options instead.',
	}).warn();
	const controller = new UmbTryExecuteController(host, resource);
	const response = await controller.tryExecute({ disableNotifications: false });
	controller.destroy();
	return response as UmbDataSourceResponse<T>;
}
