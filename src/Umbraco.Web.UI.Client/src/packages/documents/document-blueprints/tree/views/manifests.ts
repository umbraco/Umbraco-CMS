import { UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS } from '../constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'treeView',
		kind: 'classic',
		alias: 'Umb.TreeView.DocumentBlueprint.Classic',
		name: 'Document Blueprint Classic Tree View',
		forTrees: [UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS],
	},
	{
		type: 'treeView',
		kind: 'table',
		alias: 'Umb.TreeView.DocumentBlueprint.Table',
		name: 'Document Blueprint Table Tree View',
		forTrees: [UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS],
	},
];
