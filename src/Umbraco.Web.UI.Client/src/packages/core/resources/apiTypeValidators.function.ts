import type { ApiError, CancelError } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * Checks if the given error is an instance of ApiError.
 * @deprecated Use {UmbApiError.isUmbApiError} instead and map your object to {UmbApiError} if needed.
 */
export function isApiError(error: unknown): error is ApiError {
	new UmbDeprecation({
		deprecated: 'isApiError',
		removeInVersion: '18.0.0',
		solution: 'Use UmbApiError.isUmbApiError instead',
	});
	return (error as ApiError).name === 'ApiError';
}

/**
 * Checks if the given error is an instance of CancelError.
 * @deprecated Use {UmbApiCancelError.isUmbApiCancelError}` instead and map your object to {UmbApiCancelError} if needed.
 */
export function isCancelError(error: unknown): error is CancelError {
	new UmbDeprecation({
		deprecated: 'isCancelError',
		removeInVersion: '18.0.0',
		solution: 'Use UmbApiCancelError.isUmbApiCancelError instead',
	});
	return (error as CancelError).name === 'CancelError';
}

/**
 * Checks if the given promise is cancelable, i.e. if it has a cancel method.
 */
export function isCancelablePromise<T>(promise: unknown): promise is Promise<T> & { cancel: () => void } {
	return typeof (promise as Promise<T> & { cancel: () => void }).cancel === 'function';
}
