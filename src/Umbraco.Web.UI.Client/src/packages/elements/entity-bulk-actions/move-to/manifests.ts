import { UMB_ELEMENT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_TREE_ALIAS } from '../../tree/constants.js';
import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_MOVE,
} from '../../user-permissions/constants.js';
import { UMB_BULK_MOVE_ELEMENT_REPOSITORY_ALIAS } from './repository/constants.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityBulkAction',
		kind: 'moveTo',
		alias: 'Umb.EntityBulkAction.Element.MoveTo',
		name: 'Move Element Entity Bulk Action',
		weight: 20,
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			bulkMoveRepositoryAlias: UMB_BULK_MOVE_ELEMENT_REPOSITORY_ALIAS,
			treeAlias: UMB_ELEMENT_TREE_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_ELEMENT_COLLECTION_ALIAS,
			},
			{
				alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_ELEMENT_MOVE],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	...repositoryManifests,
];
