import { startMockServiceWorker } from './src/mocks/index.js';
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
	} else {
		appElement.serverUrl = import.meta.env.VITE_UMBRACO_API_URL;
	}

	// Example injector - supports both relative and absolute paths
	// For absolute paths, use Vite's /@fs/ prefix to access files outside the project
	if (import.meta.env.VITE_EXAMPLE_PATH) {
		const examplePath = import.meta.env.VITE_EXAMPLE_PATH;
		const importPath = examplePath.startsWith('/') ? '/@fs' + examplePath : './' + examplePath;
		import(/* @vite-ignore */ importPath + '/src/index.ts').then((js) => {
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
}

bootstrap();
