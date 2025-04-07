/* eslint-disable @typescript-eslint/no-explicit-any */
import { isApiError, isCancelablePromise, isCancelError } from './apiTypeValidators.function.js';
import type { UmbCancelablePromise } from './cancelable-promise.js';
import { UmbApiError, UmbCancelError, UmbError } from './umb-error.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { CancelablePromise } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbResourceController extends UmbControllerBase {
	/**
	 * The promise that is being executed.
	 * @protected
	 */
	protected _promise: UmbCancelablePromise<any> | CancelablePromise<any> | Promise<any>;

	constructor(host: UmbControllerHost, promise: Promise<unknown>, alias?: string) {
		super(host, alias);

		this._promise = promise;
	}

	/**
	 * Maps any error to an UmbError.
	 * @internal
	 * @param {*} error The error to map
	 * @returns {*} The mapped error
	 */
	mapToUmbError(error: unknown): UmbApiError | UmbCancelError | UmbError {
		if (isApiError(error)) {
			return UmbApiError.fromLegacyApiError(error as any);
		} else if (isCancelError(error)) {
			return UmbCancelError.fromLegacyCancelError(error);
		} else if (UmbCancelError.isUmbCancelError(error)) {
			return error;
		} else if (UmbApiError.isUmbApiError(error)) {
			return error;
		} else if (UmbError.isUmbError(error)) {
			return error;
		}
		// If the error is not an UmbError, we will just return it as is
		return new UmbError(error instanceof Error ? error.message : 'Unknown error');
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
