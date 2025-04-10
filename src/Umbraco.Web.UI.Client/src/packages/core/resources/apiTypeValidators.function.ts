import type { UmbProblemDetails } from './types';

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

/**
 * Checks if the given error is an instance of ProblemDetails.
 * @param {*} error The error to check
 * @returns {boolean} True if the error is an instance of ProblemDetails, false otherwise
 */
export function isProblemDetailsLike(error: unknown): error is UmbProblemDetails {
	return (
		typeof error === 'object' &&
		error !== null &&
		'type' in error &&
		'title' in error &&
		'status' in error &&
		(typeof (error as { detail?: unknown }).detail === 'undefined' ||
			typeof (error as { detail?: unknown }).detail === 'string') &&
		(typeof (error as { instance?: unknown }).instance === 'undefined' ||
			typeof (error as { instance?: unknown }).instance === 'string') &&
		(typeof (error as { operationStatus?: unknown }).operationStatus === 'undefined' ||
			typeof (error as { operationStatus?: unknown }).operationStatus === 'string') &&
		(typeof (error as { errors?: unknown }).errors === 'undefined' ||
			typeof (error as { errors?: unknown }).errors === 'object')
	);
}
