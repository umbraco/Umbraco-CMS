import {
	UMB_DISABLE_USER_REPOSITORY_ALIAS,
	UMB_ENABLE_USER_REPOSITORY_ALIAS,
	UMB_UNLOCK_USER_REPOSITORY_ALIAS,
	UMB_USER_REPOSITORY_ALIAS,
} from '../repository/manifests.js';
import { UMB_USER_ENTITY_TYPE } from '../types.js';
import { UmbEnableUserEntityBulkAction } from './enable/enable.action.js';
import { UmbSetGroupUserEntityBulkAction } from './set-group/set-group.action.js';
import { UmbUnlockUserEntityBulkAction } from './unlock/unlock.action.js';
import { UmbDisableUserEntityBulkAction } from './disable/disable.action.js';
import { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.User.SetGroup',
		name: 'SetGroup User Entity Bulk Action',
		weight: 400,
		api: UmbSetGroupUserEntityBulkAction,
		meta: {
			label: 'SetGroup',
			repositoryAlias: UMB_USER_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.CollectionEntityType',
				match: UMB_USER_ENTITY_TYPE,
			},
		],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.User.Enable',
		name: 'Enable User Entity Bulk Action',
		weight: 300,
		api: UmbEnableUserEntityBulkAction,
		meta: {
			label: 'Enable',
			repositoryAlias: UMB_ENABLE_USER_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.CollectionEntityType',
				match: UMB_USER_ENTITY_TYPE,
			},
		],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.User.Unlock',
		name: 'Unlock User Entity Bulk Action',
		weight: 200,
		api: UmbUnlockUserEntityBulkAction,
		meta: {
			label: 'Unlock',
			repositoryAlias: UMB_UNLOCK_USER_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.CollectionEntityType',
				match: UMB_USER_ENTITY_TYPE,
			},
		],
	},
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.User.Disable',
		name: 'Disable User Entity Bulk Action',
		weight: 100,
		api: UmbDisableUserEntityBulkAction,
		meta: {
			label: 'Disable',
			repositoryAlias: UMB_DISABLE_USER_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.CollectionEntityType',
				match: UMB_USER_ENTITY_TYPE,
			},
		],
	},
];

export const manifests = [...entityActions];
