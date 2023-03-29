/* eslint-disable @typescript-eslint/no-explicit-any */
import {
	UmbNotificationOptions,
	UmbNotificationContext,
	UMB_NOTIFICATION_CONTEXT_TOKEN,
} from '@umbraco-cms/backoffice/notification';
import { ApiError, CancelablePromise, ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export class UmbResourceController extends UmbController {
	#promise: Promise<any>;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement, promise: Promise<any>, alias?: string) {
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
	 * Extract the ProblemDetailsModel object from an ApiError.
	 *
	 * This assumes that all ApiErrors contain a ProblemDetailsModel object in their body.
	 */
	static toProblemDetailsModel(error: unknown): ProblemDetailsModel | undefined {
		if (error instanceof ApiError) {
			const errorDetails = error.body as ProblemDetailsModel;
			return errorDetails;
		} else if (error instanceof Error) {
			return {
				title: error.name,
				detail: error.message,
			};
		}

		return undefined;
	}

	/**
	 * Base execute function with a try/catch block and return a tuple with the result and the error.
	 */
	static async tryExecute<T>(promise: Promise<T>): Promise<DataSourceResponse<T>> {
		try {
			return { data: await promise };
		} catch (e) {
			return { error: UmbResourceController.toProblemDetailsModel(e) };
		}
	}

	/**
	 * Wrap the {execute} function in a try/catch block and return the result.
	 * If the executor function throws an error, then show the details in a notification.
	 */
	async tryExecuteAndNotify<T>(options?: UmbNotificationOptions): Promise<DataSourceResponse<T>> {
		const { data, error } = await UmbResourceController.tryExecute<T>(this.#promise);

		if (error) {
			if (this.#notificationContext) {
				this.#notificationContext?.peek('danger', {
					data: {
						headline: error.title ?? 'Server Error',
						message: error.detail ?? 'Something went wrong',
					},
					...options,
				});
			} else {
				console.group('UmbResourceController');
				console.error(error);
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
