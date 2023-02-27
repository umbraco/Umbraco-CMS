/* eslint-disable @typescript-eslint/no-explicit-any */
import { UmbResourceController } from './resource.controller';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import type { UmbNotificationOptions } from '@umbraco-cms/notification';

export function tryExecuteAndNotify<T>(
	host: UmbControllerHostInterface,
	resource: Promise<T>,
	options?: UmbNotificationOptions<any>
) {
	return new UmbResourceController(host, resource).tryExecuteAndNotify<T>(options);
}
