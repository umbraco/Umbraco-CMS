import { UMB_DATA_TYPE_TREE_ALIAS } from '../constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'treeView',
		kind: 'classic',
		alias: 'Umb.TreeView.DataType.Classic',
		name: 'Data Type Classic Tree View',
		forTrees: [UMB_DATA_TYPE_TREE_ALIAS],
	},
	{
		type: 'treeView',
		kind: 'table',
		alias: 'Umb.TreeView.DataType.Table',
		name: 'Data Type Table Tree View',
		forTrees: [UMB_DATA_TYPE_TREE_ALIAS],
	},
];
