const { setupWorker } = window.MockServiceWorker;
import { handlers } from './browser-handlers.js';
import type { MockedRequest, setupWorker as setupWorkType, rest, StartOptions } from 'msw';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const worker = setupWorker(...handlers);

export { setupWorker };

export const onUnhandledRequest = (req: MockedRequest) => {
	if (req.url.pathname.startsWith(umbracoPath(''))) {
		console.warn('Found an unhandled %s request to %s', req.method, req.url.href);
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
			setupWorker: typeof setupWorkType;
			rest: typeof rest;
		};
	}
}
