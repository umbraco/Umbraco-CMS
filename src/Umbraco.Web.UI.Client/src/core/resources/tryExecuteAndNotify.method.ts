import { UmbControllerHostInterface } from '../controller/controller-host.mixin';
import type { ProblemDetails } from '../backend-api/models/ProblemDetails';
import { UmbResourceController } from './resource.controller';
import { UmbNotificationOptions } from 'src/core/notification';

export async function tryExecuteAndNotify<T>(
	host: UmbControllerHostInterface,
	resource: Promise<T>,
	options?: UmbNotificationOptions<any>
): Promise<{ data?: T; error?: ProblemDetails }> {
	return await new UmbResourceController(host, resource).tryExecuteAndNotify<T>(options);
}
