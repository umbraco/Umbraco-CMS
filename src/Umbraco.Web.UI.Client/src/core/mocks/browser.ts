import { MockedRequest, setupWorker } from 'msw';
import { handlers } from './browser-handlers';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const worker = setupWorker(...handlers);

export const onUnhandledRequest = (req: MockedRequest) => {
	if (req.url.pathname.startsWith(umbracoPath(''))) {
		console.warn('Found an unhandled %s request to %s', req.method, req.url.href);
	}
};

export const startMockServiceWorker = () =>
	worker.start({
		onUnhandledRequest,
		quiet: import.meta.env.VITE_MSW_QUIET === 'on',
	});
