import { UMB_DOCUMENT_TREE_ALIAS } from '../constants.js';
import { UMB_DOCUMENT_VARIANT_STATE_VALUE_TYPE } from '../../variant-state/value-type/constants.js';
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
					field: 'variants',
					label: '#general_status',
					valueType: UMB_DOCUMENT_VARIANT_STATE_VALUE_TYPE,
				},
			],
		},
	},
];
