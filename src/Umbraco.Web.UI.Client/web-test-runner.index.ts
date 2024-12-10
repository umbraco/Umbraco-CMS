import { handlers } from './src/mocks/browser-handlers.js';
import { onUnhandledRequest } from './src/mocks/index.js';

const { setupWorker, rest } = window.MockServiceWorker;

const worker = setupWorker(...handlers);

worker.start({
	onUnhandledRequest,
});
