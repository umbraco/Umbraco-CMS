/* eslint-disable @typescript-eslint/no-explicit-any */
import {
	UmbNotificationOptions,
	UmbNotificationService,
	UmbNotificationDefaultData,
	UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN,
} from '@umbraco-cms/notification';
import { ApiError, CancelablePromise, ProblemDetailsModel } from '@umbraco-cms/backend-api';
import { UmbController, UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';

export class UmbResourceController extends UmbController {
	#promise: Promise<any>;

	#notificationService?: UmbNotificationService;

	constructor(host: UmbControllerHostInterface, promise: Promise<any>, alias?: string) {
		super(host, alias);

		this.#promise = promise;

		new UmbContextConsumerController(host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (_instance) => {
			this.#notificationService = _instance;
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
	static async tryExecute<T>(promise: Promise<T>): Promise<{ data?: T; error?: ProblemDetailsModel }> {
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
	async tryExecuteAndNotify<T>(
		options?: UmbNotificationOptions<any>
	): Promise<{ data?: T; error?: ProblemDetailsModel }> {
		const { data, error } = await UmbResourceController.tryExecute<T>(this.#promise);

		if (error) {
			const data: UmbNotificationDefaultData = {
				headline: error.title ?? 'Server Error',
				message: error.detail ?? 'Something went wrong',
			};

			if (this.#notificationService) {
				this.#notificationService?.peek('danger', { data, ...options });
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
