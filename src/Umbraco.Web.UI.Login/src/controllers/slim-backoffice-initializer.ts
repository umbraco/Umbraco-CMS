import { UmbBundleExtensionInitializer, UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import {
	UmbAppEntryPointExtensionInitializer,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UUIIconRegistryEssential } from '@umbraco-cms/backoffice/external/uui';
import { UmbServerConnection, UmbServerContext } from '@umbraco-cms/backoffice/server';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

// We import what we need from the Backoffice app.
// In the future the login screen app will be a part of the Backoffice app, and we will not need to import these.
import '@umbraco-cms/backoffice/localization';

/**
 * This is the initializer for the slim backoffice.
 * It is responsible for initializing the backoffice and only the extensions that is needed to run the login screen.
 */
export class UmbSlimBackofficeController extends UmbControllerBase {
	constructor(host: UmbElement) {
		super(host);

		new UmbBundleExtensionInitializer(host, umbExtensionsRegistry);

		// Attach the UUIIconRegistry to the host so that it can be used in the login screen.
		new UUIIconRegistryEssential().attach(host);

		// Add the essential styles to the host so that it can be used in the login screen.
		host.classList.add('uui-text');
		host.classList.add('uui-font');
	}

	async register(host: UmbElement, cacheBuster?: string) {
		// Get the server URL and backoffice path from the host.
		const serverUrl = window.location.origin;
		const serverConnection = new UmbServerConnection(host, serverUrl);

		// Server context the registrator reads from; carries the host cache-buster. Built as a local so the
		// published config type tolerates the extra `cacheBuster` field (consumed by the runtime backoffice).
		const serverContextConfig = {
			backofficePath: '/umbraco',
			serverUrl,
			serverConnection,
			cacheBuster,
		};
		new UmbServerContext(this, serverContextConfig);

		// Register the public extensions for the slim backoffice.
		await new UmbServerExtensionRegistrator(this, umbExtensionsRegistry)
			.registerPublicExtensions()
			.catch((error) => {
				console.error(`Failed to register public extensions for the slim backoffice.`, error);
			});

		new UmbAppEntryPointExtensionInitializer(host, umbExtensionsRegistry);
	}
}
