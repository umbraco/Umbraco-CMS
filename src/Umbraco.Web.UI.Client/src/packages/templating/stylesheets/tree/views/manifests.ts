import { UMB_STYLESHEET_TREE_ALIAS } from '../constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'treeView',
		kind: 'classic',
		alias: 'Umb.TreeView.Stylesheet.Classic',
		name: 'Stylesheet Classic Tree View',
		forTrees: [UMB_STYLESHEET_TREE_ALIAS],
	},
	{
		type: 'treeView',
		kind: 'table',
		alias: 'Umb.TreeView.Stylesheet.Table',
		name: 'Stylesheet Table Tree View',
		forTrees: [UMB_STYLESHEET_TREE_ALIAS],
	},
];
