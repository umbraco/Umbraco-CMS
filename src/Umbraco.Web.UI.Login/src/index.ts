import '@umbraco-ui/uui-css/dist/uui-css.css';
import '@umbraco-ui/uui';
import { startMockServiceWorker } from "./mocks";

if (import.meta.env.MODE === 'development') {
  startMockServiceWorker();
}

import './localization/localize.element.js';
import './auth.element.js';
