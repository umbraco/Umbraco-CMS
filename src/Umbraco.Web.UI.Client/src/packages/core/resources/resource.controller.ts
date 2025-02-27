/* eslint-disable @typescript-eslint/no-explicit-any */
import { UMB_AUTH_CONTEXT } from '../auth/index.js';
import { isApiError, isCancelError, isCancelablePromise } from './apiTypeValidators.function.js';
import type { XhrRequestOptions } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UMB_NOTIFICATION_CONTEXT, type UmbNotificationOptions } from '@umbraco-cms/backoffice/notification';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import {
	ApiError,
	CancelablePromise,
	CancelError,
	type ProblemDetails,
} from '@umbraco-cms/backoffice/external/backend-api';

export class UmbResourceController extends UmbControllerBase {
	#promise: Promise<any>;

	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;

	constructor(host: UmbControllerHost, promise: Promise<any>, alias?: string) {
		super(host, alias);

		this.#promise = promise;

		new UmbContextConsumerController(host, UMB_NOTIFICATION_CONTEXT, (_instance) => {
			this.#notificationContext = _instance;
		});

		new UmbContextConsumerController(host, UMB_AUTH_CONTEXT, (_instance) => {
			this.#authContext = _instance;
		});
	}

	override hostConnected(): void {
		// Do nothing
	}

	override hostDisconnected(): void {
		this.cancel();
	}

	/**
	 * Base execute function with a try/catch block and return a tuple with the result and the error.
	 * @param promise
	 */
	static async tryExecute<T>(promise: Promise<T>): Promise<UmbDataSourceResponse<T>> {
		try {
			return { data: await promise };
		} catch (error) {
			if (isApiError(error) || isCancelError(error)) {
				return { error };
			}

			console.error('Unknown error', error);
			throw new Error('Unknown error');
		}
	}

	/**
	 * Wrap the {tryExecute} function in a try/catch block and return the result.
	 * If the executor function throws an error, then show the details in a notification.
	 * @param options
	 */
	async tryExecuteAndNotify<T>(options?: UmbNotificationOptions): Promise<UmbDataSourceResponse<T>> {
		const { data, error } = await UmbResourceController.tryExecute<T>(this.#promise);

		if (error) {
			/**
			 * Determine if we want to show a notification or just log the error to the console.
			 * If the error is not a recognizable system error (i.e. a HttpError), then we will show a notification
			 * with the error details using the default notification options.
			 */
			if (isCancelError(error)) {
				// Cancelled - do nothing
				return {};
			} else {
				console.groupCollapsed('ApiError caught in UmbResourceController');
				console.error('Request failed', error.request);
				console.error('Request body', error.body);
				console.error('Error', error);

				let problemDetails: ProblemDetails | null = null;

				// ApiError - body could hold a ProblemDetails from the server
				if (typeof error.body !== 'undefined' && !!error.body) {
					try {
						(error as any).body = problemDetails = typeof error.body === 'string' ? JSON.parse(error.body) : error.body;
					} catch (e) {
						console.error('Error parsing error body (expected JSON)', e);
					}
				}

				/**
				 * Check if the operation status ends with `ByNotification` and if so, don't show a notification
				 * This is a special case where the operation was cancelled by the server and the client gets a notification header instead.
				 */
				let isCancelledByNotification = false;
				if (
					problemDetails?.operationStatus &&
					typeof problemDetails.operationStatus === 'string' &&
					problemDetails.operationStatus.endsWith('ByNotification')
				) {
					isCancelledByNotification = true;
				}

				// Go through the error status codes and act accordingly
				switch (error.status ?? 0) {
					case 401: {
						// See if we can get the UmbAuthContext and let it know the user is timed out
						if (this.#authContext) {
							this.#authContext.timeOut();
						} else {
							// If we can't get the auth context, show a notification
							this.#notificationContext?.peek('warning', {
								data: {
									headline: 'Session Expired',
									message: 'Your session has expired. Please refresh the page.',
								},
							});
						}
						break;
					}
					case 500:
						// Server Error

						if (!isCancelledByNotification && this.#notificationContext) {
							let headline = problemDetails?.title ?? error.name ?? 'Server Error';
							let message = 'A fatal server error occurred. If this continues, please reach out to your administrator.';

							// Special handling for ObjectCacheAppCache corruption errors, which we are investigating
							if (
								problemDetails?.detail?.includes('ObjectCacheAppCache') ||
								problemDetails?.detail?.includes('Umbraco.Cms.Infrastructure.Scoping.Scope.DisposeLastScope()')
							) {
								headline = 'Please restart the server';
								message =
									'The Umbraco object cache is corrupt, but your action may still have been executed. Please restart the server to reset the cache. This is a work in progress.';
							}

							this.#notificationContext.peek('danger', {
								data: {
									headline,
									message,
								},
								...options,
							});
						}
						break;
					default:
						// Other errors
						if (!isCancelledByNotification && this.#notificationContext) {
							this.#notificationContext.peek('danger', {
								data: {
									headline: problemDetails?.title ?? error.name ?? 'Server Error',
									message: problemDetails?.detail ?? error.message ?? 'Something went wrong',
									structuredList: problemDetails?.errors
										? (problemDetails.errors as Record<string, Array<unknown>>)
										: undefined,
								},
								...options,
							});
						}
				}

				console.groupEnd();
			}
		}

		return { data, error };
	}

	/**
	 * Make an XHR request.
	 * @param host The controller host for this controller to be appended to.
	 * @param options The options for the XHR request.
	 */
	static xhrRequest<T>(options: XhrRequestOptions): CancelablePromise<T> {
		const baseUrl = options.baseUrl || '/umbraco';

		const promise = new CancelablePromise<T>(async (resolve, reject, onCancel) => {
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
						// TODO: [JOV] This has to be changed into our own error type, when we have a chance to introduce a breaking change in the future.
						const error = new ApiError(
							{
								method: options.method,
								url: `${baseUrl}${options.url}`,
							},
							{
								body: xhr.responseText,
								ok: false,
								status: xhr.status,
								statusText: xhr.statusText,
								url: xhr.responseURL,
							},
							xhr.statusText,
						);
						reject(error);
					}
				} catch {
					// This most likely happens when the response is not JSON
					reject(new Error(`Failed to make request: ${xhr.statusText}`));
				}
			};

			xhr.onerror = () => {
				// TODO: [JOV] This has to be changed into our own error type, when we have a chance to introduce a breaking change in the future.
				const error = new ApiError(
					{
						method: options.method,
						url: `${baseUrl}${options.url}`,
					},
					{
						body: xhr.responseText,
						ok: false,
						status: xhr.status,
						statusText: xhr.statusText,
						url: xhr.responseURL,
					},
					xhr.statusText,
				);
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
				// TODO: [JOV] This has to be changed into our own error type, when we have a chance to introduce a breaking change in the future.
				reject(new CancelError('Request was cancelled.'));
			});
		});

		return promise;
	}

	/**
	 * Cancel all resources that are currently being executed by this controller if they are cancelable.
	 *
	 * This works by checking if the promise is a CancelablePromise and if so, it will call the cancel method.
	 *
	 * This is useful when the controller is being disconnected from the DOM.
	 * @see CancelablePromise
	 * @see https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal
	 * @see https://developer.mozilla.org/en-US/docs/Web/API/AbortController
	 */
	cancel(): void {
		if (isCancelablePromise(this.#promise)) {
			this.#promise.cancel();
		}
	}

	override destroy(): void {
		super.destroy();
		this.cancel();
	}
}
