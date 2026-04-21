import { UMB_DOCUMENT_TYPE_TREE_ALIAS } from '../constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'treeView',
		kind: 'classic',
		alias: 'Umb.TreeView.DocumentType.Classic',
		name: 'Document Type Classic Tree View',
		forTrees: [UMB_DOCUMENT_TYPE_TREE_ALIAS],
	},
	{
		type: 'treeView',
		kind: 'card',
		alias: 'Umb.TreeView.DocumentType.Card',
		name: 'Document Type Card Tree View',
		forTrees: [UMB_DOCUMENT_TYPE_TREE_ALIAS],
	},
];
