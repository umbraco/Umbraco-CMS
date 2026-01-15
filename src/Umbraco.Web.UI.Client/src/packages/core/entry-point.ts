import { manifests as coreManifests } from './manifests.js';
import { UMB_AUTH_CONTEXT } from './auth/auth.context.token.js';
import { UmbActionEventContext } from './action/action-event.context.js';
import { UmbBackofficeNotificationContainerElement, UmbBackofficeModalContainerElement } from './components/index.js';
import { UmbInteractionMemoryContext } from './interaction-memory/index.js';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

import './collection/global-components.js';
import './entity-item/global-components.js';
import './entity-sign/components/index.js';
import './extension-registry/components/index.js';
import './menu/components/index.js';
import './property-action/components/index.js';
import './property-editor-data-source/global-components.js';
import './property-sort-mode/global-components/index.js';

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
