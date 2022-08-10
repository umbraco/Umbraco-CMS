import 'element-internals-polyfill';

import { startMockServiceWorker } from './mocks/browser';

startMockServiceWorker();
import('./app');
