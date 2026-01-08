import type { XhrRequestOptions } from './types.js';
import { UmbResourceController } from './resource.controller.js';
import { OpenAPI, type CancelablePromise } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Make an XHR request.
 * @param {XhrRequestOptions} options The options for the XHR request.
 * @returns {CancelablePromise} A promise that can be cancelled.
 */
export function tryXhrRequest<T>(options: XhrRequestOptions): CancelablePromise<T> {
	return UmbResourceController.xhrRequest<T>({
		...options,
		baseUrl: OpenAPI.BASE,
		token: OpenAPI.TOKEN as never,
	});
}
