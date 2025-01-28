import { type StartOptions, setupWorker } from 'msw/browser';
import { handlers } from './handlers';

const worker = setupWorker(...handlers);

export const startMockServiceWorker = (config?: StartOptions) => {
	return worker.start({
		onUnhandledRequest: (req) => {
      const url = new URL(req.url);
      if (url.pathname.endsWith('.svg')) return;
      console.log('Found an unhandled %s request to %s', req.method, url.href);
    },
		quiet: false,
		...config,
	});
};
