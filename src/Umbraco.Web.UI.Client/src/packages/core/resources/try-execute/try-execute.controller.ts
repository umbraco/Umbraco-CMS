import { UmbResourceController } from '../resource.controller.js';
import type { UmbApiResponse, UmbTryExecuteOptions } from '../types.js';
import { UmbCancelError } from '../umb-error.js';
import type { UmbApiError } from '../umb-error.js';
import { apiErrorWasNotified } from './api-error-was-notified.function.js';

export class UmbTryExecuteController<T> extends UmbResourceController<T> {
	async tryExecute(opts?: UmbTryExecuteOptions): Promise<UmbApiResponse<T>> {
		try {
			const promise = opts?.abortSignal ? this.#abortable(opts.abortSignal) : this._promise;
			return (await promise) as UmbApiResponse<T>;
		} catch (error) {
			// Error might be a legacy error, so we need to check if it is an UmbError
			const umbError = this.mapToUmbError(error);

			if (!opts?.disableNotifications) {
				this.#notifyOnError(umbError);
			}

			return {
				error: umbError,
			} as UmbApiResponse<T>;
		}
	}

	/**
	 * Settles with an UmbCancelError as soon as the signal aborts, also for a promise that cannot be cancelled.
	 * @param {AbortSignal} signal The signal to listen to.
	 * @returns {Promise<T>} The promise, or a rejection with an UmbCancelError once the signal aborts.
	 */
	#abortable(signal: AbortSignal): Promise<T> {
		return new Promise<T>((resolve, reject) => {
			const abort = () => {
				this.cancel();
				reject(new UmbCancelError('Request aborted'));
			};

			Promise.resolve(this._promise)
				.then(resolve, reject)
				.finally(() => signal.removeEventListener('abort', abort));

			if (signal.aborted) {
				abort();
			} else {
				signal.addEventListener('abort', abort, { once: true });
			}
		});
	}

	#notifyOnError(error: UmbApiError | UmbCancelError): void {
		if (!apiErrorWasNotified(error)) {
			// Cancellations, non-fatal status codes, and statuses already covered by the Umb-Notifications
			// header interceptor.
			return;
		}

		/** This is a constant on purpose, because the headline should not change. We cannot trust the error details to provide a reliable headline. */
		const headline = 'An error occurred';
		let message: string;
		let detail: string | undefined;
		let errors: Record<string, string[]> | undefined;

		const apiError = error as UmbApiError;

		// Check if we can extract problem details from the error
		if (apiError.problemDetails) {
			// UmbProblemDetails, show notification
			message = apiError.problemDetails.title;
			detail = apiError.problemDetails.detail;
			errors = apiError.problemDetails.errors;

			// Special handling for ObjectCacheAppCache corruption errors, which we are investigating
			if (
				apiError.problemDetails.detail?.includes('ObjectCacheAppCache') ||
				apiError.problemDetails.detail?.includes('Umbraco.Cms.Infrastructure.Scoping.Scope.DisposeLastScope()')
			) {
				message = 'Please restart the server';
				detail =
					'The Umbraco object cache is corrupt, but your action may still have been executed. Please restart the server to reset the cache. This is a work in progress.';
			}
		} else {
			// Unknown error, show notification
			message = apiError instanceof Error ? apiError.message : 'An unknown error occurred.';
		}

		this._peekError({ headline, message, detail, errors });
		console.error('[UmbTryExecuteController] Error in request:', error);
	}
}
