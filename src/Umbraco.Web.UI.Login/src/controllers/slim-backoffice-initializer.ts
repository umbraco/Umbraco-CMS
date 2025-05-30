import { UmbBundleExtensionInitializer, UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import {
	UmbAppEntryPointExtensionInitializer,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UUIIconRegistryEssential } from '@umbraco-cms/backoffice/external/uui';
import { UmbServerConnection, UmbServerContext } from '@umbraco-cms/backoffice/server';

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

		// Get the server URL and backoffice path from the host.
		const serverUrl = window.location.origin;
		const serverConnection = new UmbServerConnection(host, serverUrl);

		// Create the server context for the slim backoffice.
		// This is needed by the UmbServerExtensionRegistrator to register the extensions.
		new UmbServerContext(this, {
			backofficePath: '/umbraco',
			serverUrl,
			serverConnection: serverConnection,
		});

		new UmbBundleExtensionInitializer(host, umbExtensionsRegistry);
		new UmbAppEntryPointExtensionInitializer(host, umbExtensionsRegistry);

		// Register the public extensions for the slim backoffice.
		new UmbServerExtensionRegistrator(this, umbExtensionsRegistry).registerPublicExtensions().catch((error) => {
			console.error(`Failed to register public extensions for the slim backoffice.`, error);
		});

		// Attach the UUIIconRegistry to the host so that it can be used in the login screen.
		new UUIIconRegistryEssential().attach(host);

		// Add the essential styles to the host so that it can be used in the login screen.
		host.classList.add('uui-text');
		host.classList.add('uui-font');
	}
}
