import { UmbAppElement } from './app/app.element';
import { startMockServiceWorker } from './core/mocks/browser';

if (import.meta.env.VITE_UMBRACO_USE_MSW === 'on') {
	startMockServiceWorker();
}

const appElement = new UmbAppElement();
const isMocking = import.meta.env.VITE_UMBRACO_USE_MSW === 'on';

if (!isMocking) {
	appElement.serverUrl = import.meta.env.VITE_UMBRACO_API_URL;
}

if (import.meta.env.DEV) {
	appElement.backofficePath = '/';
}

appElement.bypassAuth = isMocking;

document.body.appendChild(appElement);
