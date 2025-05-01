import { isProblemDetailsLike } from './apiTypeValidators.function.js';
import { UmbResourceController } from './resource.controller.js';
import type { UmbApiResponse, UmbTryExecuteOptions } from './types.js';
import { UmbApiError, UmbCancelError } from './umb-error.js';

export class UmbTryExecuteController<T> extends UmbResourceController<T> {
	#abortSignal?: AbortSignal;

	async tryExecute(opts?: UmbTryExecuteOptions): Promise<UmbApiResponse<T>> {
		try {
			if (opts?.abortSignal) {
				this.#abortSignal = opts.abortSignal;
				this.#abortSignal.addEventListener('abort', () => this.cancel(), { once: true });
			}

			return (await this._promise) as UmbApiResponse<T>;
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

	override destroy(): void {
		if (this.#abortSignal) {
			this.#abortSignal.removeEventListener('abort', this.cancel);
		}
		super.destroy();
	}

	#notifyOnError(error: unknown) {
		if (UmbCancelError.isUmbCancelError(error)) {
			// Cancel error, do not show notification
			return;
		}

		let headline = 'An error occurred';
		let message = 'An error occurred while trying to execute the request.';
		let details: Record<string, string[]> | undefined = undefined;

		// Check if we can extract problem details from the error
		const problemDetails = UmbApiError.isUmbApiError(error)
			? error.problemDetails
			: isProblemDetailsLike(error)
				? error
				: undefined;

		if (problemDetails) {
			// UmbProblemDetails, show notification
			message = problemDetails.title;
			details = problemDetails.errors ?? undefined;
		} else {
			// Unknown error, show notification
			headline = '';
			message = error instanceof Error ? error.message : 'An unknown error occurred.';
		}

		this._peekError(headline, message, details);
	}
}
