import { UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS } from '../../repository/constants.js';
import { UMB_ELEMENT_RECYCLE_BIN_COLLECTION_ALIAS } from '../constants.js';
import { UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../../constants.js';
import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_DELETE,
} from '../../../user-permissions/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_ENTITY_TYPE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/entity';
import type { ManifestCollectionActionEmptyRecycleBinKind } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<ManifestCollectionActionEmptyRecycleBinKind> = [
	{
		type: 'collectionAction',
		kind: 'emptyRecycleBin',
		alias: 'Umb.CollectionAction.Element.EmptyRecycleBin',
		name: 'Element Collection Empty Recycle Bin Action',
		meta: {
			label: '#actions_emptyrecyclebin',
			recycleBinRepositoryAlias: UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_ELEMENT_RECYCLE_BIN_COLLECTION_ALIAS,
			},
			{
				alias: UMB_ENTITY_TYPE_CONDITION_ALIAS,
				match: UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE,
			},
			{
				alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_ELEMENT_DELETE],
			},
		],
	},
];
