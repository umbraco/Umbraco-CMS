import { UmbAppElement } from '@umbraco-cms/backoffice/app';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const appElement = new UmbAppElement();
appElement.backofficePath = '/';

//#region Vite Mock Setup
if (import.meta.env.VITE_UMBRACO_USE_MSW === 'on') {
	const { startMockServiceWorker } = await import(/* @vite-ignore */ './src/mocks/index.js');
	appElement.bypassAuth = true;
	startMockServiceWorker();
} else {
	appElement.serverUrl = import.meta.env.VITE_UMBRACO_API_URL;
}

// Example injector:
if (import.meta.env.VITE_EXAMPLE_PATH) {
	import(/* @vite-ignore */ './' + import.meta.env.VITE_EXAMPLE_PATH + '/index.ts').then((js) => {
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
//#endregion

document.body.append(appElement);
