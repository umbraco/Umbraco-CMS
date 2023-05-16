import { USER_GROUP_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbDeleteUserGroupEntityBulkAction } from './delete/delete.action';
import type { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'user-group';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.UserGroup.Delete',
		name: 'Delete User Group Entity Bulk Action',
		weight: 400,
		meta: {
			label: 'Delete',
			repositoryAlias: USER_GROUP_REPOSITORY_ALIAS,
			api: UmbDeleteUserGroupEntityBulkAction,
		},
		conditions: {
			entityType,
		},
	},
];

export const manifests = [...entityActions];
