import { UmbBackofficeNotificationContainerElement, UmbBackofficeModalContainerElement } from './components';
import { manifests as debugManifests } from './debug/manifests';
import { manifests as propertyActionManifests } from './property-actions/manifests';
import { manifests as propertyEditorManifests } from './property-editors/manifests';
import { manifests as modalManifests } from './modal/common/manifests';
import { UmbStoreExtensionInitializer } from './store';

import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export * from './action';
export * from './collection';
export * from './components';
export * from './content-type';
export * from './debug';
export * from './entity-action';
export * from './entity-bulk-action';
export * from './extension-registry';
export * from './id';
export * from './menu';
export * from './modal';
export * from './notification';
export * from './picker-input';
export * from './property-actions';
export * from './property-editors';
export * from './section';
export * from './sorter';
export * from './store';
export * from './tree';
export * from './variant';
export * from './workspace';

const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...debugManifests,
	...propertyActionManifests,
	...propertyEditorManifests,
	...modalManifests,
];

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	new UmbStoreExtensionInitializer(host);

	extensionRegistry.registerMany(manifests);

	const notificationContainerElement = new UmbBackofficeNotificationContainerElement();
	host.appendChild(notificationContainerElement);

	const modalContainerElement = new UmbBackofficeModalContainerElement();
	host.appendChild(modalContainerElement);

	new UmbContextProviderController(host, UMB_NOTIFICATION_CONTEXT_TOKEN, new UmbNotificationContext());
	new UmbContextProviderController(host, UMB_MODAL_CONTEXT_TOKEN, new UmbModalContext(host));
};
