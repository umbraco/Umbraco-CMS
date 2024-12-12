import { handlers } from './src/mocks/browser-handlers.js';
import { onUnhandledRequest, setupWorker } from './src/mocks/index.js';

const worker = setupWorker(...handlers);

worker.start({
	onUnhandledRequest,
	quiet: true,
});
