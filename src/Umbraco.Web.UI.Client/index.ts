import { UmbAppElement } from './src/apps/app/app.element.js';
import { startMockServiceWorker } from './src/mocks/index.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

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


// Example injector:
if(import.meta.env.VITE_EXAMPLE_PATH) {
	import(/* @vite-ignore */ './'+import.meta.env.VITE_EXAMPLE_PATH+'/index.ts').then((js) => {
		if (js) {
			Object.keys(js).forEach((key) => {
				const value = js[key];

				if (Array.isArray(value)) {
					umbExtensionsRegistry.registerMany(value);
				} else if (typeof value === 'object') {
					umbExtensionsRegistry.register(value);
				}
			});
		}
	});

}
