import { UMB_MEDIA_TYPE_TREE_ALIAS } from '../constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'treeView',
		kind: 'classic',
		alias: 'Umb.TreeView.MediaType.Classic',
		name: 'Media Type Classic Tree View',
		forTrees: [UMB_MEDIA_TYPE_TREE_ALIAS],
	},
	{
		type: 'treeView',
		kind: 'table',
		alias: 'Umb.TreeView.MediaType.Table',
		name: 'Media Type Table Tree View',
		forTrees: [UMB_MEDIA_TYPE_TREE_ALIAS],
	},
];
