import { USER_GROUP_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UmbDeleteUserGroupEntityBulkAction } from './delete/delete.action.js';
import type { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'user-group';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.UserGroup.Delete',
		name: 'Delete User Group Entity Bulk Action',
		weight: 400,
		api: UmbDeleteUserGroupEntityBulkAction,
		meta: {
			label: 'Delete',
			repositoryAlias: USER_GROUP_REPOSITORY_ALIAS,
		},
		conditions: [{
			alias: 'Umb.Condition.CollectionEntityType',
			match: entityType,
		}],
	},
];

export const manifests = [...entityActions];
