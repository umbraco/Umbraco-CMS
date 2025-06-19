import { UmbResourceController } from '../resource.controller.js';
import type { UmbApiResponse, UmbTryExecuteOptions } from '../types.js';
import { UmbCancelError } from '../umb-error.js';
import type { UmbApiError } from '../umb-error.js';

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

	#notifyOnError(error: UmbApiError | UmbCancelError): void {
		if (UmbCancelError.isUmbCancelError(error)) {
			// Cancel error, do not show notification
			return;
		}

		let headline = 'An error occurred';
		let message = 'A fatal server error occurred. If this continues, please reach out to your administrator.';
		let details: Record<string, string[]> | undefined = undefined;

		const apiError = error as UmbApiError;

		// Check if we can extract problem details from the error
		if (apiError.problemDetails) {
			if (apiError.problemDetails.status === 401) {
				// Unauthorized error, show no notification
				// the user will see a login screen instead
				return;
			}

			if (apiError.problemDetails.status === 404) {
				// Not found error, show no notification
				// the user will see a 404 page instead, or otherwise the UI will handle it
				return;
			}

			// UmbProblemDetails, show notification
			message = apiError.problemDetails.title;
			details = apiError.problemDetails.errors ?? undefined;

			// Special handling for ObjectCacheAppCache corruption errors, which we are investigating
			if (
				apiError.problemDetails.detail?.includes('ObjectCacheAppCache') ||
				apiError.problemDetails.detail?.includes('Umbraco.Cms.Infrastructure.Scoping.Scope.DisposeLastScope()')
			) {
				headline = 'Please restart the server';
				message =
					'The Umbraco object cache is corrupt, but your action may still have been executed. Please restart the server to reset the cache. This is a work in progress.';
			}
		} else {
			// Unknown error, show notification
			message = apiError instanceof Error ? apiError.message : 'An unknown error occurred.';
		}

		this._peekError(headline, message, details);
		console.error('[UmbTryExecuteController] Error in request:', error);
	}
}
