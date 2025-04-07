import { UmbTryExecuteController } from './try-execute.controller.js';
import { UmbCancelablePromise } from './cancelable-promise.js';
import { UmbApiError } from './umb-error.js';
import type { XhrRequestOptions } from './types.js';
import { OpenAPI } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * Make an XHR request.
 * This function is a wrapper around XMLHttpRequest to provide a consistent API for making requests.
 * It supports cancelable promises, progress events, and custom headers.
 * @param {UmbControllerHost} host The host to use for the request.
 * @param {XhrRequestOptions} options The options for the request.
 * @returns {Promise<UmbDataSourceResponse<T>>} A promise that resolves with the response data or rejects with an error.
 * @template T The type of the response data.
 */
export async function tryXhrRequest<T>(
	host: UmbControllerHost,
	options: XhrRequestOptions,
): Promise<UmbDataSourceResponse<T>> {
	const promise = createXhrRequest({
		...options,
		baseUrl: OpenAPI.BASE,
		token: OpenAPI.TOKEN as never,
	});
	const controller = new UmbTryExecuteController(host, promise);
	const response = await controller.tryExecute(options);
	controller.destroy();
	return response as UmbDataSourceResponse<T>;
}

/**
 * Create an XHR request.
 * @param {XhrRequestOptions} options The options for the request.
 * @returns {UmbCancelablePromise<T>} A cancelable promise that resolves with the response data or rejects with an error.
 * @template T The type of the response data.
 * @internal
 */
function createXhrRequest<T>(options: XhrRequestOptions): UmbCancelablePromise<T> {
	const baseUrl = options.baseUrl || '/umbraco';

	return new UmbCancelablePromise<T>(async (resolve, reject, onCancel) => {
		const xhr = new XMLHttpRequest();
		xhr.open(options.method, `${baseUrl}${options.url}`, true);

		// Set default headers
		if (options.token) {
			const token = typeof options.token === 'function' ? await options.token() : options.token;
			if (token) {
				xhr.setRequestHeader('Authorization', `Bearer ${token}`);
			}
		}

		// Infer Content-Type header based on body type
		if (options.body instanceof FormData) {
			// Note: 'multipart/form-data' is automatically set by the browser for FormData
		} else {
			xhr.setRequestHeader('Content-Type', 'application/json');
		}

		// Set custom headers
		if (options.headers) {
			for (const [key, value] of Object.entries(options.headers)) {
				xhr.setRequestHeader(key, value);
			}
		}

		xhr.upload.onprogress = (event) => {
			if (options.onProgress) {
				options.onProgress(event);
			}
		};

		xhr.onload = () => {
			try {
				if (xhr.status >= 200 && xhr.status < 300) {
					if (options.responseHeader) {
						const response = xhr.getResponseHeader(options.responseHeader);
						resolve(response as T);
					} else {
						resolve(JSON.parse(xhr.responseText));
					}
				} else {
					const error = new UmbApiError(xhr.statusText, xhr.status, xhr, {
						title: xhr.statusText,
						type: 'ApiError',
						status: xhr.status,
					});
					reject(error);
				}
			} catch {
				// This most likely happens when the response is not JSON
				reject(
					new UmbApiError(`Failed to make request: ${xhr.statusText}`, xhr.status, xhr, {
						title: xhr.statusText,
						type: 'ApiError',
						status: xhr.status,
					}),
				);
			}
		};

		xhr.onerror = () => {
			const error = new UmbApiError(xhr.statusText, xhr.status, xhr, {
				title: xhr.statusText,
				type: 'ApiError',
				status: xhr.status,
			});
			reject(error);
		};

		if (!onCancel.isCancelled) {
			// Handle body based on Content-Type
			if (options.body instanceof FormData) {
				xhr.send(options.body);
			} else {
				xhr.send(JSON.stringify(options.body));
			}
		}

		onCancel(() => {
			xhr.abort();
		});
	});
}
