import { UMB_USER_GROUP_COLLECTION_ALIAS } from '../collection/index.js';
import { UMB_USER_GROUP_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UmbDeleteUserGroupEntityBulkAction } from './delete/delete.action.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import type { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.UserGroup.Delete',
		name: 'Delete User Group Entity Bulk Action',
		weight: 400,
		api: UmbDeleteUserGroupEntityBulkAction,
		meta: {
			label: 'Delete',
			repositoryAlias: UMB_USER_GROUP_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_GROUP_COLLECTION_ALIAS,
			},
		],
	},
];

export const manifests = [...entityActions];
