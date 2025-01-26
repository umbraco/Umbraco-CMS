export interface XhrRequestOptions {
	baseUrl?: string;
	method: string;
	url: string;
	body?: unknown;
	token?: string | (() => string | Promise<string>);
	headers?: Record<string, string>;
	responseHeader?: string;
	onProgress?: (event: ProgressEvent) => void;
}
