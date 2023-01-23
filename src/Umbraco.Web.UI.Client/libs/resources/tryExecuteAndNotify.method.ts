/* eslint-disable @typescript-eslint/no-explicit-any */
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import type { UmbNotificationOptions } from '../../src/core/notification';
import { UmbResourceController } from './resource.controller';

export function tryExecuteAndNotify<T>(
	host: UmbControllerHostInterface,
	resource: Promise<T>,
	options?: UmbNotificationOptions<any>
) {
	return new UmbResourceController(host, resource).tryExecuteAndNotify<T>(options);
}
