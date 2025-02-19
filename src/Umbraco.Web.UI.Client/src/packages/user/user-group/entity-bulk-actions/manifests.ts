import { UMB_USER_GROUP_COLLECTION_ALIAS } from '../collection/index.js';
import { UMB_USER_GROUP_DETAIL_REPOSITORY_ALIAS, UMB_USER_GROUP_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../entity.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityBulkAction',
		kind: 'delete',
		alias: 'Umb.EntityBulkAction.UserGroup.Delete',
		name: 'Delete User Group Entity Bulk Action',
		weight: 400,
		forEntityTypes: [UMB_USER_GROUP_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_USER_GROUP_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_USER_GROUP_DETAIL_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_GROUP_COLLECTION_ALIAS,
			},
		],
	},
];
