import { type MockedRequest, type StartOptions, setupWorker } from 'msw';
import { handlers } from './handlers';

const worker = setupWorker(...handlers);

export const onUnhandledRequest = (req: MockedRequest) => {
	console.log('Found an unhandled %s request to %s', req.method, req.url.href);
};

export const startMockServiceWorker = (config?: StartOptions) => {
	return worker.start({
		onUnhandledRequest,
		quiet: false,
		...config,
	});
};
