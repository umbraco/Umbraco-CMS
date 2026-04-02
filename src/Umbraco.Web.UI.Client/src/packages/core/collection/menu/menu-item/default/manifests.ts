import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_COLLECTION_MENU_ITEM_DEFAULT_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.CollectionMenuItem.Default',
	matchKind: 'default',
	matchType: 'collectionMenuItem',
	manifest: {
		type: 'collectionMenuItem',
		api: () => import('./default-collection-menu-item.context.js'),
		element: () => import('./default-collection-menu-item.element.js'),
	},
};

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	UMB_COLLECTION_MENU_ITEM_DEFAULT_KIND_MANIFEST,
];
