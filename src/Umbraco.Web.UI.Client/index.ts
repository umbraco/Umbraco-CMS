import { UmbAppElement } from './src/apps/app/app.element';
import { startMockServiceWorker } from './src/shared/mocks';

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

const CORE_PACKAGES = [
	import('./src/packages/core/umbraco-package'),
	import('./src/packages/settings/umbraco-package'),
	import('./src/packages/documents/umbraco-package'),
	import('./src/packages/media/umbraco-package'),
	import('./src/packages/members/umbraco-package'),
	import('./src/packages/translation/umbraco-package'),
	import('./src/packages/users/umbraco-package'),
	import('./src/packages/packages/umbraco-package'),
	import('./src/packages/search/umbraco-package'),
	import('./src/packages/templating/umbraco-package'),
	import('./src/packages/umbraco-news/umbraco-package'),
	import('./src/packages/tags/umbraco-package'),
];

appElement.localPackages = CORE_PACKAGES;

document.body.appendChild(appElement);
