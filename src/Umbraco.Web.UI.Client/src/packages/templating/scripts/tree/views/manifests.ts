import { UMB_SCRIPT_TREE_ALIAS } from '../constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'treeView',
		kind: 'classic',
		alias: 'Umb.TreeView.Script.Classic',
		name: 'Script Classic Tree View',
		forTrees: [UMB_SCRIPT_TREE_ALIAS],
	},
	{
		type: 'treeView',
		kind: 'table',
		alias: 'Umb.TreeView.Script.Table',
		name: 'Script Table Tree View',
		forTrees: [UMB_SCRIPT_TREE_ALIAS],
	},
];
