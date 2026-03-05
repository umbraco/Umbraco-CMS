import { startMockServiceWorker } from './src/mocks/index.js';
import { UmbAppElement } from '@umbraco-cms/backoffice/app';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

/**
 *
 */
async function bootstrap() {
	const appElement = new UmbAppElement();
	appElement.backofficePath = '/';

	if (import.meta.env.VITE_UMBRACO_USE_MSW === 'on') {
		appElement.bypassAuth = true;

		const mockSet = localStorage.getItem('umb:mockSet') || import.meta.env.VITE_MOCK_SET || 'default';
		await startMockServiceWorker({
			mockSet,
			useCustomServiceWorker: true,
		});

		// Register mock set switcher header app
		const { manifests } = await import('./src/mocks/app-extension/manifests.js');
		umbExtensionsRegistry.registerMany(manifests);
	} else {
		appElement.serverUrl = import.meta.env.VITE_UMBRACO_API_URL;
	}

	document.body.append(appElement);

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
}

bootstrap();
