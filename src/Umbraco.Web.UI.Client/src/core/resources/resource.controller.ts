/* eslint-disable @typescript-eslint/no-explicit-any */
import { UmbController } from '../controller/controller.class';
import { UmbControllerHostInterface } from '../controller/controller-host.mixin';
import { UmbContextConsumerController } from '../context-api/consume/context-consumer.controller';
import {
	UmbNotificationOptions,
	UmbNotificationService,
	UmbNotificationDefaultData,
	UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN,
} from '../notification';
import { ApiError, CancelablePromise, ProblemDetails } from '@umbraco-cms/backend-api';

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
	 * Extract the ProblemDetails object from an ApiError.
	 *
	 * This assumes that all ApiErrors contain a ProblemDetails object in their body.
	 */
	static toProblemDetails(error: unknown): ProblemDetails | undefined {
		if (error instanceof ApiError) {
			const errorDetails = error.body as ProblemDetails;
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
	static async tryExecute<T>(promise: Promise<T>): Promise<{ data?: T; error?: ProblemDetails }> {
		try {
			return { data: await promise };
		} catch (e) {
			return { error: UmbResourceController.toProblemDetails(e) };
		}
	}

	/**
	 * Wrap the {execute} function in a try/catch block and return the result.
	 * If the executor function throws an error, then show the details in a notification.
	 */
	async tryExecuteAndNotify<T>(options?: UmbNotificationOptions<any>): Promise<{ data?: T; error?: ProblemDetails }> {
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
