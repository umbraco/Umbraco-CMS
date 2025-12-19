const { setupWorker } = window.MockServiceWorker;
import { handlers } from './browser-handlers.js';
import type { setupWorker as setupWorkerType, StartOptions } from 'msw/browser';
import type { http as httpType, HttpResponse as HttpResponseType, delay as delayType } from 'msw';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const worker = setupWorker(...handlers);

export { setupWorker };

export const onUnhandledRequest = (request: Request) => {
	const url = new URL(request.url);
	if (url.pathname.startsWith(umbracoPath(''))) {
		console.warn('Found an unhandled %s request to %s', request.method, request.url);
	}
};

export const startMockServiceWorker = (config?: StartOptions) =>
	worker.start({
		onUnhandledRequest,
		...config,
	});

declare global {
	interface Window {
		MockServiceWorker: {
			setupWorker: typeof setupWorkerType;
			http: typeof httpType;
			HttpResponse: typeof HttpResponseType;
			delay: typeof delayType;
		};
	}
}
