import { handlers } from './src/mocks/browser-handlers.js';
import { onUnhandledRequest } from './src/mocks/index.js';

const { setupWorker } = window.MockServiceWorker;

const worker = setupWorker(...handlers);

worker.start({
	onUnhandledRequest,
});
