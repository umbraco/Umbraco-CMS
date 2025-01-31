import { UMB_USER_GROUP_COLLECTION_ALIAS } from '../collection/index.js';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../entity.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.UserGroup.Delete',
		name: 'Delete User Group Entity Bulk Action',
		weight: 400,
		api: () => import('./delete/delete.action.js'),
		forEntityTypes: [UMB_USER_GROUP_ENTITY_TYPE],
		meta: {
			label: 'Delete',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_GROUP_COLLECTION_ALIAS,
			},
		],
	},
];
