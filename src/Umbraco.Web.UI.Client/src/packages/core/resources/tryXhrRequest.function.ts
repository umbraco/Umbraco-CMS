import { UMB_AUTH_CONTEXT } from '../auth/auth.context.token.js';
import type { XhrRequestOptions } from './types.js';
import { UmbResourceController } from './resource.controller.js';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { OpenAPI, type CancelablePromise } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Make an XHR request.
 * @param host The controller host for this controller to be appended to.
 * @param options The options for the XHR request.
 */
export function tryXhrRequest<T>(host: UmbControllerHost, options: XhrRequestOptions): CancelablePromise<T> {
	return UmbResourceController.xhrRequest<T>({
		...options,
		baseUrl: OpenAPI.BASE,
		async token() {
			const contextConsumer = new UmbContextConsumerController(host, UMB_AUTH_CONTEXT).asPromise();
			const authContext = await contextConsumer;
			return authContext.getLatestToken();
		},
	});
}
