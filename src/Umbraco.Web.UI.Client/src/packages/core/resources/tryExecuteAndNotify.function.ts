import { UmbTryExecuteController } from './try-execute.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
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
