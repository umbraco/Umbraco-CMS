/**
 * Checks if the given error is an instance of ApiError.
 * @param {*} error The error to check
 * @returns {boolean} True if the error is an instance of ApiError, false otherwise
 * @deprecated Use {UmbApiError.isUmbApiError} instead and map your object to {UmbApiError} if needed.
 */
export function isApiError(error: unknown): error is { body?: string; status?: number; request?: unknown } {
	return typeof error === 'object' && error !== null && 'body' in error && 'status' in error && 'request' in error;
}

/**
 * Checks if the given error is an instance of CancelError.
 * @param {*} error The error to check
 * @returns {boolean} True if the error is an instance of CancelError, false otherwise
 * @deprecated Use {UmbApiCancelError.isUmbApiCancelError}` instead and map your object to {UmbApiCancelError} if needed.
 */
export function isCancelError(error: unknown): error is Error {
	return error instanceof Error && (error.name === 'CancelError' || (error as Error).message === 'Request aborted');
}

/**
 * Checks if the given promise is cancelable, i.e. if it has a cancel method.
 * @param {*} promise The promise to check
 * @returns {boolean} True if the promise is cancelable, false otherwise
 */
export function isCancelablePromise<T>(promise: unknown): promise is Promise<T> & { cancel: () => void } {
	return typeof (promise as Promise<T> & { cancel: () => void }).cancel === 'function';
}
