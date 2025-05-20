import type { UmbApiError, UmbCancelError, UmbError } from './umb-error.js';
export type * from './data-api/types.js';

export interface XhrRequestOptions extends UmbTryExecuteOptions {
	baseUrl?: string;
	method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH' | 'HEAD' | 'OPTIONS';
	url: string;
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
	 * Signal object to cancel the request.
	 */
	abortSignal?: AbortSignal;
}

export type UmbApiWithErrorResponse = {
	error?: UmbError | UmbApiError | UmbCancelError | Error;
};

/**
 * UmbApiResponse is a type that represents the response from an API call.
 * It can either be a successful response with data or an error response.
 * @template T The type of the response data.
 * @property {T} data The data returned from the API.
 * @property {UmbError | UmbApiError | UmbCancelError | Error} error The error returned from the API.
 */
export type UmbApiResponse<T = unknown> = T & UmbApiWithErrorResponse;
