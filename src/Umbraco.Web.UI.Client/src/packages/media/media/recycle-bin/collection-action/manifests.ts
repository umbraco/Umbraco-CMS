import {
	UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS,
	UMB_MEDIA_RECYCLE_BIN_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
} from '../constants.js';
import {
	UMB_COLLECTION_ALIAS_CONDITION,
	UMB_COLLECTION_HAS_ITEMS_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionAction',
		kind: 'emptyRecycleBin',
		name: 'Media Collection Empty Recycle Bin Action',
		alias: 'Umb.CollectionAction.Media.EmptyRecycleBin',
		meta: {
			label: '#actions_emptyrecyclebin',
			recycleBinRepositoryAlias: UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_MEDIA_RECYCLE_BIN_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
			},
			{
				alias: UMB_COLLECTION_HAS_ITEMS_CONDITION_ALIAS,
			},
		],
	},
];
