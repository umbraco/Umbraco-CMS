import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import UmbDefaultTreeItemContext from './tree-item-default.context.js';
import UmbDefaultTreeItemElement from './tree-item-default.element.js';

export const UMB_TREE_ITEM_DEFAULT_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.TreeItem.Default',
	matchKind: 'default',
	matchType: 'treeItem',
	manifest: {
		type: 'treeItem',
		api: UmbDefaultTreeItemContext,
		element: UmbDefaultTreeItemElement,
	},
};

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [UMB_TREE_ITEM_DEFAULT_KIND_MANIFEST];
