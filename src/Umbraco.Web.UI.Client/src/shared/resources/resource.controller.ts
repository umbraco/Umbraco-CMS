/* eslint-disable @typescript-eslint/no-explicit-any */
import {
	UmbNotificationContext,
	UMB_NOTIFICATION_CONTEXT_TOKEN,
	UmbNotificationOptions,
} from '@umbraco-cms/backoffice/notification';
import { ApiError, CancelError, CancelablePromise } from '@umbraco-cms/backoffice/backend-api';
import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export class UmbResourceController extends UmbBaseController {
	#promise: Promise<any>;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost, promise: Promise<any>, alias?: string) {
		super(host, alias);

		this.#promise = promise;

		new UmbContextConsumerController(host, UMB_NOTIFICATION_CONTEXT_TOKEN, (_instance) => {
			this.#notificationContext = _instance;
		});
	}

	hostConnected(): void {
		// Do nothing
	}

	hostDisconnected(): void {
		this.cancel();
	}

	/**
	 * Base execute function with a try/catch block and return a tuple with the result and the error.
	 */
	static async tryExecute<T>(promise: Promise<T>): Promise<DataSourceResponse<T>> {
		try {
			return { data: await promise };
		} catch (error) {
			if (error instanceof ApiError || error instanceof CancelError) {
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
	async tryExecuteAndNotify<T>(options?: UmbNotificationOptions): Promise<DataSourceResponse<T>> {
		const { data, error } = await UmbResourceController.tryExecute<T>(this.#promise);

		if (error) {
			/**
			 * Determine if we want to show a notification or just log the error to the console.
			 * If the error is not a recognizable system error (i.e. a HttpError), then we will show a notification
			 * with the error details using the default notification options.
			 */
			if (error instanceof CancelError) {
				// Cancelled - do nothing
				return {};
			} else {
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
					case 401:
						// Unauthorized
						console.log('Unauthorized');

						// TODO: Do not remove the token here but instead let whatever is listening to the event decide what to do
						localStorage.removeItem('tokenResponse');

						// TODO: Show a modal dialog to login either by bubbling an event to UmbAppElement or by showing a modal directly
						this.#notificationContext?.peek('warning', {
							data: {
								headline: 'Session Expired',
								message: 'Your session has expired. Please refresh the page.',
							},
						});
						break;
					default:
						// Other errors
						if (this.#notificationContext) {
							this.#notificationContext.peek('danger', {
								data: {
									headline: error.body.title ?? error.name ?? 'Server Error',
									message: error.body.detail ?? error.message ?? 'Something went wrong',
								},
								...options,
							});
						} else {
							console.group('UmbResourceController');
							console.error(error);
							console.groupEnd();
						}
				}
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
	cancel() {
		if (this.#promise instanceof CancelablePromise) {
			this.#promise.cancel();
		}
	}

	destroy() {
		super.destroy();
		this.cancel();
	}
}
