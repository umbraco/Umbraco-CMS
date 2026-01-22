import { UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS } from '../../repository/constants.js';
import { UMB_ELEMENT_RECYCLE_BIN_COLLECTION_ALIAS } from '../constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestCollectionActionEmptyRecycleBinKind } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<ManifestCollectionActionEmptyRecycleBinKind> = [
	{
		type: 'collectionAction',
		kind: 'emptyRecycleBin',
		name: 'Element Collection Empty Recycle Bin Action',
		alias: 'Umb.CollectionAction.Element.EmptyRecycleBin',
		meta: {
			label: '#actions_emptyrecyclebin',
			recycleBinRepositoryAlias: UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_ELEMENT_RECYCLE_BIN_COLLECTION_ALIAS,
			},
		],
	},
];
