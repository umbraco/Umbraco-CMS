export interface XhrRequestOptions extends UmbTryExecuteOptions {
	baseUrl?: string;
	method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH' | 'HEAD' | 'OPTIONS';
	url: string;
	body?: unknown;
	token?: string | (() => string | Promise<string>);
	headers?: Record<string, string>;
	responseHeader?: string;
	onProgress?: (event: ProgressEvent) => void;
}

export interface UmbProblemDetails {
	type: string;
	title: string;
	status: number;
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
