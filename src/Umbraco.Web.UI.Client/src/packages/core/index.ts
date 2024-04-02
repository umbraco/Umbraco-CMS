import { UmbBackofficeNotificationContainerElement, UmbBackofficeModalContainerElement } from './components/index.js';
import { UmbActionEventContext } from './action/action-event.context.js';
import { manifests as coreManifests } from './manifests.js';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbExtensionsApiInitializer, type UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

export * from './collection/index.js';
export * from './localization/index.js';
export * from './action/index.js';
export * from './components/index.js';
export * from './content-type/index.js';
export * from './debug/index.js';
export * from './entity-action/index.js';
export * from './entity-bulk-action/index.js';
export * from './extension-registry/index.js';
export * from './id/index.js';
export * from './menu/index.js';
export * from './modal/index.js';
export * from './notification/index.js';
export * from './picker-input/index.js';
export * from './property-action/index.js';
export * from './property-editor/index.js';
export * from './section/index.js';
export * from './store/index.js';
export * from './tree/index.js';
export * from './variant/index.js';
export * from './workspace/index.js';
export * from './culture/index.js';
export * from './temporary-file/index.js';

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
