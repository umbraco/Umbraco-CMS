import { type MockedRequest, type setupWorker as setupWorkType, type rest, type StartOptions, setupWorker } from 'msw';
import { handlers } from './handlers/index.js';

const worker = setupWorker(...handlers);

export const onUnhandledRequest = (req: MockedRequest) => {
	console.log('Found an unhandled %s request to %s', req.method, req.url.href);
};

export const startMockServiceWorker = (config?: StartOptions) => {
	worker.start({
		onUnhandledRequest,
		quiet: false,
		...config,
	});
};

declare global {
	interface Window {
		MockServiceWorker: {
			setupWorker: typeof setupWorkType;
			rest: typeof rest;
		};
	}
}
