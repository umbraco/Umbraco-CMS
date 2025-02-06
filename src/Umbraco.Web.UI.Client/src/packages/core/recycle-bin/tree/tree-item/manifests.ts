import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_TREE_ITEM_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/tree';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.TreeItem.RecycleBin',
		matchType: 'treeItem',
		matchKind: 'recycleBin',
		manifest: {
			...UMB_TREE_ITEM_DEFAULT_KIND_MANIFEST.manifest,
			type: 'treeItem',
			kind: 'recycleBin',
			api: () => import('./recycle-bin-tree-item.context.js'),
		},
	},
];
