import { isApiError, isCancelablePromise, isCancelError, isProblemDetailsLike } from './apiTypeValidators.function.js';
import { UmbApiError, UmbCancelError } from './umb-error.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbResourceController<T = unknown> extends UmbControllerBase {
	/**
	 * The promise that is being executed.
	 * @protected
	 */
	protected _promise;

	constructor(host: UmbControllerHost, promise: PromiseLike<T>, alias?: string) {
		super(host, alias);

		this._promise = promise;
	}

	/**
	 * Maps any error to an UmbError.
	 * @internal
	 * @param {*} error The error to map
	 * @returns {*} The mapped error
	 */
	mapToUmbError(error: unknown): UmbApiError | UmbCancelError {
		if (isProblemDetailsLike(error)) {
			return new UmbApiError(error.detail ?? error.title, error.status, null, error);
		} else if (isApiError(error)) {
			return UmbApiError.fromLegacyApiError(error as never);
		} else if (isCancelError(error)) {
			return UmbCancelError.fromLegacyCancelError(error);
		} else if (UmbCancelError.isUmbCancelError(error)) {
			return error;
		} else if (UmbApiError.isUmbApiError(error)) {
			return error;
		}

		// If the error is not recognizable, for example if it has no ProblemDetails body, we will return a generic UmbApiError.
		// This is to ensure that we always return an UmbApiError, so we can handle it in a consistent way.
		return new UmbApiError(error instanceof Error ? error.message : 'Unknown error', 0, null, {
			status: 0,
			title: 'Unknown error',
			detail: error instanceof Error ? error.message : 'Unknown error',
			errors: undefined,
			type: 'error',
			stack: error instanceof Error ? error.stack : undefined,
		});
	}

	/**
	 * Cancel all resources that are currently being executed by this controller if they are cancelable.
	 *
	 * This works by checking if the promise is a CancelablePromise and if so, it will call the cancel method.
	 *
	 * This is useful when the controller is being disconnected from the DOM.
	 * @see CancelablePromise
	 * @see https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal
	 * @see https://developer.mozilla.org/en-US/docs/Web/API/AbortController
	 */
	cancel(): void {
		if (isCancelablePromise(this._promise)) {
			this._promise.cancel();
		}
	}

	override hostDisconnected(): void {
		super.hostDisconnected();
		this.cancel();
	}

	override destroy(): void {
		super.destroy();
		this.cancel();
	}

	protected async _peekError(headline: string, message: string, details: unknown) {
		// Store the host for usage in the following async context
		const host = this._host;

		// This late importing is done to avoid circular reference
		(await import('@umbraco-cms/backoffice/notification')).umbPeekError(host, {
			headline,
			message,
			details,
		});
	}
}
