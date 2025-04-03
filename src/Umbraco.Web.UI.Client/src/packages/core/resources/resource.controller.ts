/* eslint-disable @typescript-eslint/no-explicit-any */
import { UMB_AUTH_CONTEXT } from '../auth/index.js';
import { UmbDeprecation } from '../utils/deprecation/deprecation.js';
import { UmbApiError, UmbCancelError, type UmbError } from './umb-error.js';
import { isApiError, isCancelError, isCancelablePromise } from './apiTypeValidators.function.js';
import type { XhrRequestOptions } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbNotificationOptions } from '@umbraco-cms/backoffice/notification';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import {
	ApiError,
	CancelablePromise,
	CancelError,
	type ProblemDetails,
} from '@umbraco-cms/backoffice/external/backend-api';

export class UmbResourceController extends UmbControllerBase {
	#promise: Promise<any>;

	constructor(host: UmbControllerHost, promise: Promise<any>, alias?: string) {
		super(host, alias);

		this.#promise = promise;
	}

	/**
	 * Handle errors
	 * @internal
	 * @param {any} error The error to handle.
	 * @returns {UmbDataSourceResponse<T>} The response object with the error.
	 * @template T The type of the data returned from the API.
	 */
	async handleUmbErrors<T>(error: UmbError | UmbApiError | UmbCancelError | Error): Promise<UmbDataSourceResponse<T>> {
		if (!error) return {};

		/**
		 * Determine if we want to show a notification or just log the error to the console.
		 * If the error is not a recognizable system error (i.e. a HttpError), then we will show a notification
		 * with the error details using the default notification options.
		 */
		if (UmbCancelError.isUmbCancelError(error)) {
			// Cancelled - do nothing
			return { error };
		} else if (UmbApiError.isUmbApiError(error)) {
			// UmbApiError - handle it
			return this.handleUmbApiError(error);
		} else {
			// UmbError or unknown error - log it to the console
			console.error('Error occured', error);
			return { error };
		}
	}

	/**
	 * Handle errors from the API and show a notification if needed.
	 * @internal
	 * @param {UmbApiError} error The error to handle.
	 * @param {boolean} shouldNotify Whether to show a notification or not. Default is false.
	 * @returns {UmbDataSourceResponse<T>} The response object with the error.
	 * @template T The type of the data returned from the API.
	 * @see {@link UmbResourceController.handleUmbErrors<T>}
	 */
	async handleUmbApiError<T>(error: UmbApiError, shouldNotify: boolean = false): Promise<UmbDataSourceResponse<T>> {
		console.error('UmbError caught in UmbResourceController', error);

		/**
		 * Check if the operation status ends with `ByNotification` and if so, don't show a notification
		 * This is a special case where the operation was cancelled by the server and the client gets a notification header instead.
		 */
		if (
			error.problemDetails.operationStatus &&
			typeof error.problemDetails.operationStatus === 'string' &&
			error.problemDetails.operationStatus.endsWith('ByNotification')
		) {
			shouldNotify = false;
		}

		// Go through the error status codes and act accordingly
		switch (error.status ?? 0) {
			case 401: {
				// See if we can get the UmbAuthContext and let it know the user is timed out
				const authContext = await this.getContext(UMB_AUTH_CONTEXT, { preventTimeout: true });
				if (!authContext) {
					throw new Error('Could not get the auth context');
				}
				authContext.timeOut();
				break;
			}
			case 500:
				// Server Error

				if (shouldNotify) {
					let headline = error.problemDetails.title ?? error.name ?? 'Server Error';
					let message = 'A fatal server error occurred. If this continues, please reach out to your administrator.';

					// Special handling for ObjectCacheAppCache corruption errors, which we are investigating
					if (
						error.problemDetails.detail?.includes('ObjectCacheAppCache') ||
						error.problemDetails.detail?.includes('Umbraco.Cms.Infrastructure.Scoping.Scope.DisposeLastScope()')
					) {
						headline = 'Please restart the server';
						message =
							'The Umbraco object cache is corrupt, but your action may still have been executed. Please restart the server to reset the cache. This is a work in progress.';
					}

					this.#peekError(headline, message, error.problemDetails.errors ?? error.problemDetails.detail);
				}
				break;
			default:
				// Other errors
				if (shouldNotify) {
					const headline = error.problemDetails.title ?? error.name ?? 'Server Error';
					this.#peekError('', headline, error.problemDetails.errors ?? error.problemDetails.detail);
				}
		}

		return { error };
	}

	async #peekError(headline: string, message: string, details: unknown) {
		// This late importing is done to avoid circular reference [NL]
		(await import('@umbraco-cms/backoffice/notification')).umbPeekError(this, {
			headline,
			message,
			details,
		});
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

	override hostDisconnected(): void {
		super.hostDisconnected();
		this.cancel();
	}

	override destroy(): void {
		super.destroy();
		this.cancel();
	}

	/**
	 * Base execute function with a try/catch block and return a tuple with the result and the error.
	 * @deprecated This method is deprecated and will be removed in Umbraco 18. Use the {UmbTryExecuteController} instead.
	 * @param promise
	 */
	static async tryExecute<T>(promise: Promise<T>): Promise<UmbDataSourceResponse<T>> {
		new UmbDeprecation({
			deprecated: 'UmbResourceController.tryExecute',
			removeInVersion: '18.0.0',
			solution: 'Use the UmbTryExecuteController instead.',
		});
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
	 * @deprecated This method is deprecated and will be removed in Umbraco 18. Use the {UmbTryExecuteAndNotifyController} instead.
	 * @param options
	 */

	async tryExecuteAndNotify<T>(options?: UmbNotificationOptions): Promise<UmbDataSourceResponse<T>> {
		new UmbDeprecation({
			deprecated: 'UmbResourceController.tryExecuteAndNotify',
			removeInVersion: '18.0.0',
			solution: 'Use the UmbTryExecuteAndNotifyController instead.',
		});

		const { data, error } = await UmbResourceController.tryExecute<T>(this.#promise);

		if (options) {
			new UmbDeprecation({
				deprecated: 'tryExecuteAndNotify `options` argument is deprecated.',
				removeInVersion: '17.0.0',
				solution: 'Use the method without arguments.',
			}).warn();
		}

		if (error) {
			/**
			 * Determine if we want to show a notification or just log the error to the console.
			 * If the error is not a recognizable system error (i.e. a HttpError), then we will show a notification
			 * with the error details using the default notification options.
			 */
			if (isCancelError(error)) {
				// Cancelled - do nothing
				return { error: new UmbCancelError(error.message) };
			} else if (isApiError(error)) {
				console.groupCollapsed('ApiError caught in UmbResourceController');
				console.error('Request failed', error.request);
				console.error('Request body', error.body);
				console.error('Error', error);
				console.groupEnd();

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
						const authContext = await this.getContext(UMB_AUTH_CONTEXT, { preventTimeout: true });
						if (!authContext) {
							throw new Error('Could not get the auth context');
						}
						authContext.timeOut();
						break;
					}
					case 500:
						// Server Error

						if (!isCancelledByNotification) {
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

							this.#peekError(headline, message, problemDetails?.errors ?? problemDetails?.detail);
						}
						break;
					default:
						// Other errors
						if (!isCancelledByNotification) {
							const headline = problemDetails?.title ?? error.name ?? 'Server Error';
							this.#peekError('', headline, problemDetails?.errors ?? problemDetails?.detail);
						}
				}
			} else {
				// UmbError or unknown error - log it to the console
				console.error('Unknown error', error);
				this.#peekError('Error', error.message ?? 'Unknown error', []);
			}
		}

		return { data, error };
	}

	/**
	 * Make an XHR request.
	 * @deprecated This method is deprecated and will be removed in Umbraco 18. Use the {UmbXhrRequestController} instead.
	 * @param host The controller host for this controller to be appended to.
	 * @param options The options for the XHR request.
	 */
	static xhrRequest<T>(options: XhrRequestOptions): CancelablePromise<T> {
		new UmbDeprecation({
			deprecated: 'UmbResourceController.xhrRequest',
			removeInVersion: '18.0.0',
			solution: 'Use the UmbXhrRequestController instead.',
		});

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

		if (options.abortSignal) {
			options.abortSignal.addEventListener('abort', () => {
				promise.cancel();
			});
		}

		return promise;
	}
}
