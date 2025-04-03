import type { XhrRequestOptions } from './types.js';
import { UmbTryXhrRequestController } from './try-xhr-request.controller.js';
import { OpenAPI } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * Make an XHR request.
 */
export async function tryXhrRequest<T>(
	host: UmbControllerHost,
	options: XhrRequestOptions,
): Promise<UmbDataSourceResponse<T>> {
	const promise = UmbTryXhrRequestController.createXhrRequest({
		...options,
		baseUrl: OpenAPI.BASE,
		token: OpenAPI.TOKEN as never,
	});
	const controller = new UmbTryXhrRequestController(host, promise);
	const response = await controller.tryXhrRequest(options.abortSignal);
	controller.destroy();
	return response as UmbDataSourceResponse<T>;
}
