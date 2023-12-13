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
import { manifests as dataTypeManifests } from './data-type/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';

import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbExtensionsApiInitializer, type UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

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
export * from './temporary-file/index.js';

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
	...dataTypeManifests,
	...treeManifests,
];

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
	new UmbExtensionsApiInitializer(host, extensionRegistry, 'globalContext', [host]);
	new UmbExtensionsApiInitializer(host, extensionRegistry, 'store', [host]);
	new UmbExtensionsApiInitializer(host, extensionRegistry, 'treeStore', [host]);
	new UmbExtensionsApiInitializer(host, extensionRegistry, 'itemStore', [host]);

	extensionRegistry.registerMany(manifests);

	const notificationContainerElement = new UmbBackofficeNotificationContainerElement();
	host.appendChild(notificationContainerElement);

	const modalContainerElement = new UmbBackofficeModalContainerElement();
	host.appendChild(modalContainerElement);

	new UmbNotificationContext(host);
	new UmbModalManagerContext(host);
};
