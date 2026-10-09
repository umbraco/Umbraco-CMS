import { UmbDefaultCollectionMenuItemContext } from './default-collection-menu-item.context.js';
import { UmbDefaultCollectionMenuItemElement } from './default-collection-menu-item.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_COLLECTION_MENU_ITEM_DEFAULT_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.CollectionMenuItem.Default',
	matchKind: 'default',
	matchType: 'collectionMenuItem',
	manifest: {
		type: 'collectionMenuItem',
		api: UmbDefaultCollectionMenuItemContext,
		element: UmbDefaultCollectionMenuItemElement,
	},
};

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	UMB_COLLECTION_MENU_ITEM_DEFAULT_KIND_MANIFEST,
];
