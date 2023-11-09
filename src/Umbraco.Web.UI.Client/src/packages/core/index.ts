import { UmbBackofficeNotificationContainerElement, UmbBackofficeModalContainerElement } from './components/index.js';
import { manifests as debugManifests } from './debug/manifests.js';
import { manifests as localizationManifests } from './localization/manifests.js';
import { manifests as propertyActionManifests } from './property-action/manifests.js';
import { manifests as propertyEditorManifests } from './property-editor/manifests.js';
import { manifests as tinyMcePluginManifests } from './property-editor/uis/tiny-mce/plugins/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as modalManifests } from './modal/common/manifests.js';
import { manifests as themeManifests } from './themes/manifests.js';
import { manifests as conditionManifests } from './extension-registry/conditions/manifests.js';
import { manifests as cultureManifests } from './culture/manifests.js';

import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbModalManagerContext, UMB_MODAL_MANAGER_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbExtensionsApiController, type UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export * from './localization/index.js';
export * from './action/index.js';
export * from './components/index.js';
export * from './content-type/index.js';
export * from './debug/index.js';
export * from './entity-action/index.js';
export * from './entity-bulk-action/index.js';
export * from './extension-registry/index.js';
export * from './id/index.js';
export * from './macro/index.js';
export * from './menu/index.js';
export * from './modal/index.js';
export * from './notification/index.js';
export * from './picker-input/index.js';
export * from './property-action/index.js';
export * from './property-editor/index.js';
export * from './section/index.js';
export * from './sorter/index.js';
export * from './store/index.js';
export * from './tree/index.js';
export * from './variant/index.js';
export * from './workspace/index.js';
export * from './culture/index.js';

const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...conditionManifests,
	...debugManifests,
	...localizationManifests,
	...propertyActionManifests,
	...propertyEditorManifests,
	...tinyMcePluginManifests,
	...collectionManifests,
	...workspaceManifests,
	...modalManifests,
	...themeManifests,
	...cultureManifests,
];

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {

	new UmbExtensionsApiController(host, extensionRegistry, 'globalContext', [host]);
	new UmbExtensionsApiController(host, extensionRegistry, 'store', [host]);
	new UmbExtensionsApiController(host, extensionRegistry, 'treeStore', [host]);
	new UmbExtensionsApiController(host, extensionRegistry, 'itemStore', [host]);

	extensionRegistry.registerMany(manifests);

	const notificationContainerElement = new UmbBackofficeNotificationContainerElement();
	host.appendChild(notificationContainerElement);

	const modalContainerElement = new UmbBackofficeModalContainerElement();
	host.appendChild(modalContainerElement);

	new UmbContextProviderController(host, UMB_NOTIFICATION_CONTEXT_TOKEN, new UmbNotificationContext());
	new UmbContextProviderController(host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, new UmbModalManagerContext(host));
};
