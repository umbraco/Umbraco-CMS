import { getSelectedExampleNames } from './mocks/examples.js';
import { startMockServiceWorker } from './mocks/index.js';
import { UmbAppElement } from '@umbraco-cms/backoffice/app';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

function getExamplePaths(): Array<string> {
	const paths = [import.meta.env.VITE_EXAMPLE_PATH, ...getSelectedExampleNames().map((name) => `examples/${name}`)];
	return [...new Set(paths.filter(Boolean))];
}

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
		// TODO: implement for the static build too. We need to be able load the mock sets
		if (import.meta.env.MODE === 'development') {
			const { manifests } = await import('./mocks/backoffice-extensions/manifests.js');
			umbExtensionsRegistry.registerMany(manifests);
		}
	} else {
		appElement.serverUrl = import.meta.env.VITE_UMBRACO_API_URL;
	}

	document.body.append(appElement);

	// Example injector:
	const examplePaths = getExamplePaths();
	Promise.allSettled(
		examplePaths.map(async (path) => {
			const js = await import(/* @vite-ignore */ './' + path + '/index.ts');
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
		}),
	).then((results) => {
		results.forEach((result, index) => {
			if (result.status === 'rejected') {
				console.warn(`Example "${examplePaths[index]}" failed to load`, result.reason);
			}
		});
	});
	//#endregion
}

bootstrap();
