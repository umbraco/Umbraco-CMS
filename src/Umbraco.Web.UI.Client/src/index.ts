import 'element-internals-polyfill';

import { startMockServiceWorker } from './mocks/browser';

if (import.meta.env.VITE_UMBRACO_USE_MSW === 'on') {
	startMockServiceWorker();
}

import('./app');
