import { startMockServiceWorker, addMockHandlers } from './src/mocks/index.js';
import { UmbAppElement } from '@umbraco-cms/backoffice/app';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

/**
 *
 */
async function bootstrap() {
	const appElement = new UmbAppElement();
	appElement.backofficePath = '/';

	//#region Vite Mock Setup
	if (import.meta.env.VITE_UMBRACO_USE_MSW === 'on') {
		appElement.bypassAuth = true;
		await startMockServiceWorker();

		// Load external MSW handlers if provided
		if (import.meta.env.VITE_EXTERNAL_MOCKS && import.meta.env.VITE_EXTERNAL_MOCKS !== '') {
			try {
				const mocks = await import(/* @vite-ignore */ '@external-mocks/handlers.ts');
				if (mocks?.handlers && Array.isArray(mocks.handlers)) {
					addMockHandlers(...mocks.handlers);
					console.log(`🎭 Registered ${mocks.handlers.length} external MSW handler(s)`);
				}
			} catch (error) {
				console.error('Failed to load external MSW handlers:', error);
			}
		}
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

	// External extension injector (load from outside the project):
	if (import.meta.env.VITE_EXTERNAL_EXTENSION) {
		try {
			const js = await import(/* @vite-ignore */ '@external-extension/index.ts');
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
		} catch (error) {
			console.error('Failed to load external extension:', error);
		}
	}
	//#endregion

	document.body.append(appElement);
}

bootstrap();
