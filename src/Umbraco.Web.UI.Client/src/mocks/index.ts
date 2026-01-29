const { setupWorker, isCommonAssetRequest } = window.MockServiceWorker;
import { handlers } from './browser-handlers.js';
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

export const startMockServiceWorker = (config?: StartOptions) =>
	worker.start({
		onUnhandledRequest,
		// Use our custom service worker wrapper that bypasses static assets
		// at the service worker level, before they reach MSW's fetch handler.
		// This avoids the expensive round-trip to the main thread for every
		// JS/CSS/image request, significantly improving startup performance.
		serviceWorker: {
			url: '/umbServiceWorker.js',
		},
		...config,
	});

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
