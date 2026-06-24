import { UMB_DOCUMENT_TREE_ALIAS } from '../constants.js';
import { UMB_DATE_TIME_VALUE_TYPE } from '@umbraco-cms/backoffice/value-type';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'treeView',
		kind: 'classic',
		alias: 'Umb.TreeView.Document.Classic',
		name: 'Document Classic Tree View',
		forTrees: [UMB_DOCUMENT_TREE_ALIAS],
	},
	{
		type: 'treeView',
		kind: 'card',
		alias: 'Umb.TreeView.Document.Card',
		name: 'Document Card Tree View',
		forTrees: [UMB_DOCUMENT_TREE_ALIAS],
	},
	{
		type: 'treeView',
		kind: 'table',
		alias: 'Umb.TreeView.Document.Table',
		name: 'Document Table Tree View',
		forTrees: [UMB_DOCUMENT_TREE_ALIAS],
		meta: {
			columns: [
				{
					field: 'createDate',
					label: '#content_createDate',
					valueType: UMB_DATE_TIME_VALUE_TYPE,
				},
			],
		},
	},
];
