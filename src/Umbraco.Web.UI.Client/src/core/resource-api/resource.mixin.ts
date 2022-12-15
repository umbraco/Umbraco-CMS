/* eslint-disable @typescript-eslint/no-explicit-any */
import { ApiError, CancelablePromise, ProblemDetails } from '@umbraco-cms/backend-api';
import type { HTMLElementConstructor } from '@umbraco-cms/models';
import { UmbNotificationOptions, UmbNotificationService, UmbNotificationDefaultData } from '@umbraco-cms/services';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

export declare class UmbResourceMixinInterface {
	tryExecute<T>(promise: CancelablePromise<T>): Promise<[T | undefined, ProblemDetails | undefined]>;
	executeAndNotify<T>(promise: CancelablePromise<T>, options?: UmbNotificationOptions<any>): Promise<T | undefined>;
	addResource(promise: CancelablePromise<any>): void;
	cancelAllResources(): void;
}

export const UmbResourceMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbResourceMixinClass extends UmbContextConsumerMixin(superClass) implements UmbResourceMixinInterface {
		#promises: CancelablePromise<any>[] = [];

		private _notificationService?: UmbNotificationService;

		connectedCallback() {
			super.connectedCallback?.();
			this.#promises.length = 0;
			this.consumeContext('umbNotificationService', (notificationService) => {
				this._notificationService = notificationService;
			});
		}

		disconnectedCallback() {
			super.disconnectedCallback?.();
			this.cancelAllResources();
		}

		addResource(promise: CancelablePromise<any>): void {
			this.#promises.push(promise);
		}

		/**
		 * Execute a given function and get the result as a promise.
		 */
		execute<T>(func: CancelablePromise<T>): Promise<T> {
			this.addResource(func);
			return func;
		}

		/**
		 * Wrap the {execute} function in a try/catch block and return a tuple with the result and the error.
		 */
		async tryExecute<T>(func: CancelablePromise<T>): Promise<[T | undefined, ProblemDetails | undefined]> {
			try {
				return [await this.execute(func), undefined];
			} catch (e) {
				return [undefined, this.#toProblemDetails(e)];
			}
		}

		/**
		 * Wrap the {execute} function in a try/catch block and return the result.
		 * If the executor function throws an error, then show the details in a notification.
		 */
		async executeAndNotify<T>(
			func: CancelablePromise<T>,
			options?: UmbNotificationOptions<any>
		): Promise<T | undefined> {
			try {
				return await this.execute(func);
			} catch (e) {
				const error = this.#toProblemDetails(e);
				if (error) {
					const data: UmbNotificationDefaultData = {
						headline: error.title ?? 'Server Error',
						message: error.detail ?? 'Something went wrong',
					};
					this._notificationService?.peek('danger', { data, ...options });
				}
			}

			return undefined;
		}

		/**
		 * Cancel all resources that are currently being executed.
		 */
		cancelAllResources() {
			this.#promises.forEach((promise) => {
				if (promise instanceof CancelablePromise) {
					promise.cancel();
				}
			});
		}

		/**
		 * Extract the ProblemDetails object from an ApiError.
		 *
		 * This assumes that all ApiErrors contain a ProblemDetails object in their body.
		 */
		#toProblemDetails(error: unknown): ProblemDetails | undefined {
			if (error instanceof ApiError) {
				const errorDetails = error.body as ProblemDetails;
				return errorDetails;
			}

			return undefined;
		}
	}

	return UmbResourceMixinClass as unknown as HTMLElementConstructor<UmbResourceMixinInterface> & T;
};
