import { UmbTryExecuteAndNotifyController } from './try-execute-and-notify.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 *
 * @param host
 * @param resource
 * @param options
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
