import { UmbResourceController } from './resource.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbNotificationOptions } from '@umbraco-cms/backoffice/notification';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 *
 * @param host
 * @param resource
 * @param options
 */
export function tryExecuteAndNotify<T>(
	host: UmbControllerHost,
	resource: Promise<T>,
	options?: UmbNotificationOptions,
): Promise<UmbDataSourceResponse<T>> {
	return new UmbResourceController(host, resource).tryExecuteAndNotify<T>(options);
}
