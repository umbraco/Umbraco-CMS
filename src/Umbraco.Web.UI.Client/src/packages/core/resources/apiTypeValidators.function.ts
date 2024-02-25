import type { ApiError, CancelError, CancelablePromise } from '@umbraco-cms/backoffice/external/backend-api';

export function isApiError(error: unknown): error is ApiError {
	return (error as ApiError).name === 'ApiError';
}

export function isCancelError(error: unknown): error is CancelError {
	return (error as CancelError).name === 'CancelError';
}

export function isCancelablePromise<T>(promise: unknown): promise is CancelablePromise<T> {
	return (promise as CancelablePromise<T>).cancel !== undefined;
}
