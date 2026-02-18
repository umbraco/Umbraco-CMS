import {
	UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
	UMB_DOCUMENT_RECYCLE_BIN_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
} from '../constants.js';
import {
	UMB_COLLECTION_ALIAS_CONDITION,
	UMB_COLLECTION_HAS_ITEMS_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'emptyRecycleBin',
		name: 'Document Collection Empty Recycle Bin Action',
		alias: 'Umb.CollectionAction.Document.EmptyRecycleBin',
		meta: {
			label: '#actions_emptyrecyclebin',
			recycleBinRepositoryAlias: UMB_DOCUMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_DOCUMENT_RECYCLE_BIN_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
			},
			{
				alias: UMB_COLLECTION_HAS_ITEMS_CONDITION_ALIAS,
			},
		],
	},
];
