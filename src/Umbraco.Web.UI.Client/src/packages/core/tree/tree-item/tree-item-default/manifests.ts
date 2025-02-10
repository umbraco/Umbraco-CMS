import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_TREE_ITEM_DEFAULT_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.TreeItem.Default',
	matchKind: 'default',
	matchType: 'treeItem',
	manifest: {
		type: 'treeItem',
		api: () => import('./tree-item-default.context.js'),
		element: () => import('./tree-item-default.element.js'),
	},
};

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [UMB_TREE_ITEM_DEFAULT_KIND_MANIFEST];
