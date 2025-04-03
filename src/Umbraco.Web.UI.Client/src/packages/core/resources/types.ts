export interface XhrRequestOptions {
	baseUrl?: string;
	method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH' | 'HEAD' | 'OPTIONS';
	url: string;
	body?: unknown;
	token?: string | (() => string | Promise<string>);
	headers?: Record<string, string>;
	responseHeader?: string;
	onProgress?: (event: ProgressEvent) => void;
	abortSignal?: AbortSignal;
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
