import { UmbCancelablePromise } from './cancelable-promise.js';
import { UmbTryExecuteController } from './try-execute.controller.js';
import type { XhrRequestOptions } from './types.js';
import { UmbApiError } from './umb-error.js';

export class UmbTryXhrRequestController extends UmbTryExecuteController {
	static createXhrRequest<T>(options: XhrRequestOptions): UmbCancelablePromise<T> {
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
}
