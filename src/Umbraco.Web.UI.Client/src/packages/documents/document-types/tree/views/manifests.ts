import { UMB_DOCUMENT_TYPE_TREE_ALIAS } from '../constants.js';
import { UMB_BOOLEAN_VALUE_TYPE } from '@umbraco-cms/backoffice/value-type';
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
	{
		type: 'treeView',
		kind: 'table',
		alias: 'Umb.TreeView.DocumentType.Table',
		name: 'Document Type Table Tree View',
		forTrees: [UMB_DOCUMENT_TYPE_TREE_ALIAS],
		meta: {
			columns: [
				{
					field: 'isElement',
					label: '#contentTypeEditor_elementType',
					valueType: UMB_BOOLEAN_VALUE_TYPE,
				},
			],
		},
	},
];
