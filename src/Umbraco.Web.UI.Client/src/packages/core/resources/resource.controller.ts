/* eslint-disable @typescript-eslint/no-explicit-any */
import { UMB_AUTH_CONTEXT } from '../auth/index.js';
import { isApiError, isCancelError, isCancelablePromise } from './apiTypeValidators.function.js';
import { html } from '@umbraco-cms/backoffice/external/lit';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UMB_NOTIFICATION_CONTEXT, type UmbNotificationOptions } from '@umbraco-cms/backoffice/notification';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export type ErrorMessageText = { property: string; messages: string[] };

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

	#buildApiErrorMessage(error: ErrorMessageText) {
		if (!error) return undefined;
		if (typeof error !== 'object') return undefined;

		const entries: Array<ErrorMessageText> = [];
		Object.entries(error).forEach(([property, message]) => {
			entries.push({ property, messages: Array.isArray(message) ? message : [message] });
		});

		const template = html` ${entries.map((e) => e.messages.map((msg: string) => html`<div>${msg}</div>`))}`;

		return template;
	}

	/**
	 * Base execute function with a try/catch block and return a tuple with the result and the error.
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
	 */
	async tryExecuteAndNotify<T>(options?: UmbNotificationOptions): Promise<UmbDataSourceResponse<T>> {
		const { data, error: _error } = await UmbResourceController.tryExecute<T>(this.#promise);
		const error: any = _error;
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
				console.group('ApiError caught in UmbResourceController');
				console.error('Request failed', error.request);
				console.error('ProblemDetails', error.body);
				console.error('Error', error);

				// ApiError - body could hold a ProblemDetails from the server
				if (typeof error.body !== 'undefined' && !!error.body) {
					try {
						(error as any).body = typeof error.body === 'string' ? JSON.parse(error.body) : error.body;
					} catch (e) {
						console.error('Error parsing error body (expected JSON)', e);
					}
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

						if (this.#notificationContext) {
							let headline = error.body?.title ?? error.name ?? 'Server Error';
							let message = 'A fatal server error occurred. If this continues, please reach out to your administrator.';

							// Special handling for ObjectCacheAppCache corruption errors, which we are investigating
							if (
								error.body?.detail?.includes('ObjectCacheAppCache') ||
								error.body?.detail?.includes('Umbraco.Cms.Infrastructure.Scoping.Scope.DisposeLastScope()')
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
						if (this.#notificationContext) {
							const message = this.#buildApiErrorMessage(error?.body?.errors);
							this.#notificationContext.peek('danger', {
								data: {
									headline: error.body?.title ?? error.name ?? 'Server Error',
									message: message ?? error.body?.detail ?? error.message ?? 'Something went wrong',
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
	 * Cancel all resources that are currently being executed by this controller if they are cancelable.
	 *
	 * This works by checking if the promise is a CancelablePromise and if so, it will call the cancel method.
	 *
	 * This is useful when the controller is being disconnected from the DOM.
	 *
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
