import '@umbraco-ui/uui-css/dist/uui-css.css';
import 'element-internals-polyfill';

import { startMockServiceWorker } from './mocks/browser';

startMockServiceWorker();
import('./app');
