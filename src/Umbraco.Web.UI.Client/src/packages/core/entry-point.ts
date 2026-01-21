import { manifests as coreManifests } from './manifests.js';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import {
	UmbBackofficeNotificationContainerElement,
	UmbBackofficeModalContainerElement,
} from '@umbraco-cms/backoffice/components';
import { UmbInteractionMemoryContext } from '@umbraco-cms/backoffice/interaction-memory';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

// Ensure the load of some package for their global components(Which are exported as part of the import-map, those where it's not have deep imports here):
import '@umbraco-cms/backoffice/collection';
import '@umbraco-cms/backoffice/entity-sign';
import '@umbraco-cms/backoffice/extension-registry';
import '@umbraco-cms/backoffice/menu';
import '@umbraco-cms/backoffice/property-action';
import '@umbraco-cms/backoffice/property-editor-data-source';
import '@umbraco-cms/backoffice/property-sort-mode';
import '@umbraco-cms/backoffice/entity-item';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	new UmbExtensionsApiInitializer(host, extensionRegistry, 'globalContext', [host]);
	new UmbExtensionsApiInitializer(host, extensionRegistry, 'store', [host]);
	/**
	 * TODO: Remove this in Umbraco 18, use the repository instead
	 */
	new UmbExtensionsApiInitializer(host, extensionRegistry, 'treeStore', [host]);
	new UmbExtensionsApiInitializer(host, extensionRegistry, 'itemStore', [host]);

	extensionRegistry.registerMany(coreManifests);

	const notificationContainerElement = new UmbBackofficeNotificationContainerElement();
	host.shadowRoot?.appendChild(notificationContainerElement);

	const modalContainerElement = new UmbBackofficeModalContainerElement();
	host.shadowRoot?.appendChild(modalContainerElement);

	new UmbNotificationContext(host);
	new UmbModalManagerContext(host);
	new UmbActionEventContext(host);
	new UmbInteractionMemoryContext(host);

	host.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
		// Initialize the auth context to let the app context know that the core module is ready
		authContext?.setInitialized();
	});
};
