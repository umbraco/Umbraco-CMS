/* eslint-disable @typescript-eslint/no-explicit-any */
import { UmbControllerHostInterface } from '../../src/core/controller/controller-host.mixin';
import { UmbResourceController } from './resource.controller';
import { UmbNotificationOptions } from 'src/core/notification';

export function tryExecuteAndNotify<T>(
	host: UmbControllerHostInterface,
	resource: Promise<T>,
	options?: UmbNotificationOptions<any>
) {
	return new UmbResourceController(host, resource).tryExecuteAndNotify<T>(options);
}
