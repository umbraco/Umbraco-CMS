import { UMB_TEMPLATE_TREE_ALIAS } from '../constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'treeView',
		kind: 'classic',
		alias: 'Umb.TreeView.Template.Classic',
		name: 'Template Classic Tree View',
		forTrees: [UMB_TEMPLATE_TREE_ALIAS],
	},
	{
		type: 'treeView',
		kind: 'table',
		alias: 'Umb.TreeView.Template.Table',
		name: 'Template Table Tree View',
		forTrees: [UMB_TEMPLATE_TREE_ALIAS],
	},
];
