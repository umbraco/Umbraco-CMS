import type { UmbApiError, UmbCancelError, UmbError } from './umb-error.js';
export type * from './data-api/types.js';

// TODO: Rename this to `UmbXhrRequestOptions` in a future version.
// eslint-disable-next-line @typescript-eslint/naming-convention
export interface XhrRequestOptions extends UmbTryExecuteOptions {
	baseUrl?: string;
	method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH' | 'HEAD' | 'OPTIONS';
	url: string;
	withCredentials?: boolean;
	body?: unknown;
	token?: string | (() => undefined | string | Promise<string | undefined>);
	headers?: Record<string, string>;
	responseHeader?: string;
	onProgress?: (event: ProgressEvent) => void;
}

export interface UmbProblemDetails {
	type: string;
	title: string;
	status: number;
	stack?: unknown;
	detail?: string;
	instance?: string;
	operationStatus?: string;
	extensions?: Record<string, unknown>;
	errors?: Record<string, string[]>;
}

export interface UmbTryExecuteOptions {
	/**
	 * If set to true, the controller will not show any notifications at all.
	 * @default false
	 */
	disableNotifications?: boolean;

	/**
	 * Signal that cancels the request. When it aborts, the call resolves straight away with an `UmbCancelError`
	 * and shows no notification. A promise with a `cancel()` method, such as an `UmbCancelablePromise`, is cancelled too.
	 * To also stop a fetch request, pass the same signal to the request.
	 */
	abortSignal?: AbortSignal;
}

export type UmbApiWithErrorResponse = {
	error?: UmbError | UmbApiError | UmbCancelError;
};

/**
 * UmbApiResponse is a type that represents the response from an API call.
 * It can either be a successful response with data or an error response.
 * @template T The type of the response data.
 * @property {T} data The data returned from the API.
 * @property {UmbError | UmbApiError | UmbCancelError | Error} error The error returned from the API.
 */
export type UmbApiResponse<T = unknown> = T & UmbApiWithErrorResponse;
