import { USER_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbUserDeleteEntityBulkAction } from './delete/delete.action';
import { UmbEnableUserEntityBulkAction } from './enable/enable.action';
import { UmbSetGroupUserEntityBulkAction } from './set-group/set-group.action';
import { UmbUnlockUserEntityBulkAction } from './unlock/unlock.action';
import { UmbDisableUserEntityBulkAction } from './disable/disable.action';
import { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'user';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.User.SetGroup',
		name: 'SetGroup User Entity Bulk Action',
		weight: 400,
		meta: {
			label: 'SetGroup',
			repositoryAlias: USER_REPOSITORY_ALIAS,
			api: UmbSetGroupUserEntityBulkAction,
		},
		conditions: {
			entityType,
		},
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.User.Enable',
		name: 'Enable User Entity Bulk Action',
		weight: 300,
		meta: {
			label: 'Enable',
			repositoryAlias: USER_REPOSITORY_ALIAS,
			api: UmbEnableUserEntityBulkAction,
		},
		conditions: {
			entityType,
		},
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.User.Unlock',
		name: 'Unlock User Entity Bulk Action',
		weight: 200,
		meta: {
			label: 'Unlock',
			repositoryAlias: USER_REPOSITORY_ALIAS,
			api: UmbUnlockUserEntityBulkAction,
		},
		conditions: {
			entityType,
		},
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.User.Disable',
		name: 'Disable User Entity Bulk Action',
		weight: 100,
		meta: {
			label: 'Disable',
			repositoryAlias: USER_REPOSITORY_ALIAS,
			api: UmbDisableUserEntityBulkAction,
		},
		conditions: {
			entityType,
		},
	},
];

export const manifests = [...entityActions];
