import { UmbResourceController } from './resource.controller.js';
import type { UmbTryExecuteOptions } from './types.js';
import { UmbApiError, UmbCancelError } from './umb-error.js';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export class UmbTryExecuteController<T> extends UmbResourceController<T> {
	#abortSignal?: AbortSignal;

	async tryExecute(opts?: UmbTryExecuteOptions): Promise<UmbDataSourceResponse<T>> {
		try {
			if (opts?.abortSignal) {
				this.#abortSignal = opts.abortSignal;
				this.#abortSignal.addEventListener('abort', () => this.cancel(), { once: true });
			}

			const response = await this._promise;

			if (response && typeof response === 'object' && 'data' in response) {
				return { ...response } as UmbDataSourceResponse<T>;
			}

			return {
				data: response,
			} as UmbDataSourceResponse<T>;
		} catch (error) {
			// Error might be a legacy error, so we need to check if it is an UmbError
			const umbError = this.mapToUmbError(error);

			if (!opts?.disableNotifications) {
				this.#notifyOnError(umbError);
			}

			return {
				error: umbError,
			} as UmbDataSourceResponse<T>;
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
		let details: Record<string, string[]> = {};

		if (UmbApiError.isUmbApiError(error)) {
			// UmbApiError, show notification
			headline = error.problemDetails.title ?? error.name;
			message = error.problemDetails.detail ?? error.message;
			details = error.problemDetails.errors ?? {};
		} else {
			// Unknown error, show notification
			headline = '';
			message = error instanceof Error ? error.message : 'An unknown error occurred.';
		}

		this._peekError(headline, message, details);
	}
}
