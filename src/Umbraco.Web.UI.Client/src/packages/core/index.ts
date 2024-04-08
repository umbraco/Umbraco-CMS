import { UmbBackofficeNotificationContainerElement, UmbBackofficeModalContainerElement } from './components/index.js';
import { UmbActionEventContext } from './action/action-event.context.js';
import { manifests as coreManifests } from './manifests.js';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbExtensionsApiInitializer, type UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	new UmbExtensionsApiInitializer(host, extensionRegistry, 'globalContext', [host]);
	new UmbExtensionsApiInitializer(host, extensionRegistry, 'store', [host]);
	new UmbExtensionsApiInitializer(host, extensionRegistry, 'treeStore', [host]);
	new UmbExtensionsApiInitializer(host, extensionRegistry, 'itemStore', [host]);

	extensionRegistry.registerMany(coreManifests);

	const notificationContainerElement = new UmbBackofficeNotificationContainerElement();
	host.appendChild(notificationContainerElement);

	const modalContainerElement = new UmbBackofficeModalContainerElement();
	host.appendChild(modalContainerElement);

	new UmbNotificationContext(host);
	new UmbModalManagerContext(host);
	new UmbActionEventContext(host);
};
