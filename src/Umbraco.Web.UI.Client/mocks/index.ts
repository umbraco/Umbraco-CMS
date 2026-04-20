const { setupWorker, isCommonAssetRequest } = window.MockServiceWorker;
import { handlers } from './browser-handlers.js';
import { umbMockManager } from './mock-manager.js';
import type { setupWorker as setupWorkerType, StartOptions } from 'msw/browser';
import type {
	http as httpType,
	HttpResponse as HttpResponseType,
	delay as delayType,
	isCommonAssetRequest as isCommonAssetRequestType,
} from 'msw';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const worker = setupWorker(...handlers);

export { setupWorker };

export const onUnhandledRequest = (request: Request) => {
	// Skip common static asset requests (JS/TS modules, CSS, images, fonts, etc.)
	// This restores MSW's default behavior which is opted-out when using a custom callback.
	if (isCommonAssetRequest(request)) {
		return;
	}

	const url = new URL(request.url);
	if (url.pathname.startsWith(umbracoPath(''))) {
		console.warn('Found an unhandled %s request to %s', request.method, request.url);
	}
};

/**
 * Options for starting the mock service worker.
 */
export interface UmbMockServiceWorkerOptions extends StartOptions {
	/**
	 * The name of the mock data set to use.
	 * @default 'default'
	 */
	mockSet?: string;

	/**
	 * Whether to use the custom service worker that bypasses static assets.
	 * Set to true for Vite development (faster startup).
	 * Tests should use false since they serve files from a different location.
	 * @default false
	 */
	useCustomServiceWorker?: boolean;
}

/**
 * Starts the mock service worker with the specified options.
 * @param config - Configuration options including the mock set to use.
 */
export const startMockServiceWorker = async (config?: UmbMockServiceWorkerOptions) => {
	const { mockSet = 'default', useCustomServiceWorker = false, ...mswConfig } = config ?? {};

	// Initialize mock data with the specified set
	await umbMockManager.initialize(mockSet);

	return worker.start({
		onUnhandledRequest,
		// Use custom service worker wrapper that bypasses static assets at the
		// service worker level, avoiding expensive round-trips to main thread.
		// This significantly improves startup performance in Vite development.
		...(useCustomServiceWorker && {
			serviceWorker: {
				url: '/umbServiceWorker.js',
			},
		}),
		...mswConfig,
	});
};

// Re-export convenience functions for tests
export { useMockSet, clearMockData } from './mock-manager.js';

declare global {
	interface Window {
		MockServiceWorker: {
			setupWorker: typeof setupWorkerType;
			http: typeof httpType;
			HttpResponse: typeof HttpResponseType;
			delay: typeof delayType;
			isCommonAssetRequest: typeof isCommonAssetRequestType;
		};
	}
}
