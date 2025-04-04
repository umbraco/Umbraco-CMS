import { UmbTryExecuteAndNotifyController } from './try-execute-and-notify.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * @deprecated Use the {tryExecute} function instead and handle the error in the caller.
 * This function is kept for backwards compatibility and will be removed in a future version.
 */
export async function tryExecuteAndNotify<T>(
	host: UmbControllerHost,
	resource: Promise<T>,
): Promise<UmbDataSourceResponse<T>> {
	const controller = new UmbTryExecuteAndNotifyController(host, resource);
	const response = await controller.tryExecuteAndNotify();
	controller.destroy();
	return response as UmbDataSourceResponse<T>;
}
