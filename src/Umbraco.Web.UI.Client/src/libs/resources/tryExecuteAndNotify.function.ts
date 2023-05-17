/* eslint-disable @typescript-eslint/no-explicit-any */
import { UmbResourceController } from './resource.controller';
import { UmbControllerHostElement } from 'src/libs/controller-api';
import type { UmbNotificationOptions } from 'src/libs/notification';

export function tryExecuteAndNotify<T>(
	host: UmbControllerHostElement,
	resource: Promise<T>,
	options?: UmbNotificationOptions
) {
	return new UmbResourceController(host, resource).tryExecuteAndNotify<T>(options);
}
