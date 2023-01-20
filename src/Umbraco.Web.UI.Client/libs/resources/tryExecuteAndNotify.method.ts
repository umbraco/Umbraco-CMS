/* eslint-disable @typescript-eslint/no-explicit-any */
import { UmbNotificationOptions } from '../../src/core/notification';
import { UmbResourceController } from './resource.controller';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export function tryExecuteAndNotify<T>(
	host: UmbControllerHostInterface,
	resource: Promise<T>,
	options?: UmbNotificationOptions<any>
) {
	return new UmbResourceController(host, resource).tryExecuteAndNotify<T>(options);
}
