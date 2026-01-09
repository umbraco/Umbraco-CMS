import { startMockServiceWorker, addMockHandlers } from './src/mocks/index.js';
import { UmbAppElement } from '@umbraco-cms/backoffice/app';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

/**
 * Registers extension manifests from a module's exports.
 */
function registerExtensionsFromModule(module: Record<string, unknown>) {
	for (const value of Object.values(module)) {
		if (Array.isArray(value)) {
			umbExtensionsRegistry.registerMany(value);
		}
	}
}

/**
 * Loads external MSW handlers if VITE_EXTERNAL_MOCKS is configured.
 */
async function loadExternalMockHandlers() {
	if (!import.meta.env.VITE_EXTERNAL_MOCKS) {
		return;
	}
	try {
		// @ts-expect-error - Dynamic alias resolved by vite.config.external.ts
		const mocks = await import(/* @vite-ignore */ '@external-mocks/handlers.ts');
		const handlers = mocks?.handlers;
		if (Array.isArray(handlers)) {
			addMockHandlers(...handlers);
			console.log(`🎭 Registered ${handlers.length} external MSW handler(s)`);
		}
	} catch (error) {
		console.error('Failed to load external MSW handlers:', error);
	}
}

/**
 * Loads external extension if VITE_EXTERNAL_EXTENSION is configured.
 */
async function loadExternalExtension() {
	if (!import.meta.env.VITE_EXTERNAL_EXTENSION) {
		return;
	}
	try {
		// @ts-expect-error - Dynamic alias resolved by vite.config.external.ts
		const module = await import(/* @vite-ignore */ '@external-extension/index.ts');
		if (module) {
			registerExtensionsFromModule(module);
		}
	} catch (error) {
		console.error('Failed to load external extension:', error);
	}
}

/**
 * Configures the app for MSW mock mode or server mode.
 */
async function configureMockOrServerMode(appElement: UmbAppElement) {
	if (import.meta.env.VITE_UMBRACO_USE_MSW !== 'on') {
		appElement.serverUrl = import.meta.env.VITE_UMBRACO_API_URL;
		return;
	}
	appElement.bypassAuth = true;
	await startMockServiceWorker();
	await loadExternalMockHandlers();
}

/**
 * Application bootstrap entry point.
 */
async function bootstrap() {
	const appElement = new UmbAppElement();
	appElement.backofficePath = '/';

	//#region Vite Mock Setup
	await configureMockOrServerMode(appElement);

	// Example injector:
	if (import.meta.env.VITE_EXAMPLE_PATH) {
		import(/* @vite-ignore */ './' + import.meta.env.VITE_EXAMPLE_PATH + '/index.ts').then((module) => {
			if (module) {
				registerExtensionsFromModule(module);
			}
		});
	}

	await loadExternalExtension();
	//#endregion

	document.body.append(appElement);
}

bootstrap();
