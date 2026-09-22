import { UMB_PARTIAL_VIEW_TREE_ALIAS } from '../constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'treeView',
		kind: 'classic',
		alias: 'Umb.TreeView.PartialView.Classic',
		name: 'Partial View Classic Tree View',
		forTrees: [UMB_PARTIAL_VIEW_TREE_ALIAS],
	},
	{
		type: 'treeView',
		kind: 'table',
		alias: 'Umb.TreeView.PartialView.Table',
		name: 'Partial View Table Tree View',
		forTrees: [UMB_PARTIAL_VIEW_TREE_ALIAS],
	},
];
