import { UmbBundleExtensionInitializer, UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import {
	UmbAppEntryPointExtensionInitializer,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UUIIconRegistryEssential } from '@umbraco-cms/backoffice/external/uui';
import { UmbServerConnection, UmbServerContext } from '@umbraco-cms/backoffice/server';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

// We import what we need from the Backoffice app.
// In the future the login screen app will be a part of the Backoffice app, and we will not need to import these.
import '@umbraco-cms/backoffice/localization';

// The third `cacheBuster` argument ships with the runtime backoffice (resolved via the importmap). This app
// type-checks against the published @umbraco-cms/backoffice, whose types may lag, so reference the constructor
// through a signature that includes the parameter.
// TODO: drop this cast and the alias once the login app consumes the backoffice by source — the local
// UmbServerExtensionRegistrator then declares the cacheBuster parameter directly.
type UmbServerExtensionRegistratorCtor = new (
	host: UmbControllerHost,
	extensionRegistry: typeof umbExtensionsRegistry,
	cacheBuster?: string,
) => UmbServerExtensionRegistrator;

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

		// Create the server context for the slim backoffice.
		// This is needed by the UmbServerExtensionRegistrator to register the extensions.
		new UmbServerContext(this, {
			backofficePath: '/umbraco',
			serverUrl,
			serverConnection: serverConnection,
		});

		// Register the public extensions for the slim backoffice.
		await new (UmbServerExtensionRegistrator as UmbServerExtensionRegistratorCtor)(this, umbExtensionsRegistry, cacheBuster)
			.registerPublicExtensions()
			.catch((error) => {
				console.error(`Failed to register public extensions for the slim backoffice.`, error);
			});

		new UmbAppEntryPointExtensionInitializer(host, umbExtensionsRegistry);
	}
}
