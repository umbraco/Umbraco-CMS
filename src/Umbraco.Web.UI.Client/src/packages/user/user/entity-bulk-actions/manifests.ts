import {
	UMB_DISABLE_USER_REPOSITORY_ALIAS,
	UMB_ENABLE_USER_REPOSITORY_ALIAS,
	UMB_UNLOCK_USER_REPOSITORY_ALIAS,
	UMB_USER_DETAIL_REPOSITORY_ALIAS,
} from '../repository/index.js';
import { UMB_USER_COLLECTION_ALIAS } from '../collection/manifests.js';
import { UmbEnableUserEntityBulkAction } from './enable/enable.action.js';
import { UmbSetGroupUserEntityBulkAction } from './set-group/set-group.action.js';
import { UmbUnlockUserEntityBulkAction } from './unlock/unlock.action.js';
import { UmbDisableUserEntityBulkAction } from './disable/disable.action.js';
import type { ManifestEntityBulkAction } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

const entityActions: Array<ManifestEntityBulkAction> = [
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.User.SetGroup',
		name: 'SetGroup User Entity Bulk Action',
		weight: 400,
		api: UmbSetGroupUserEntityBulkAction,
		meta: {
			label: 'SetGroup',
			repositoryAlias: UMB_USER_DETAIL_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
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
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
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
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
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
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
			},
		],
	},
];

export const manifests = [...entityActions];
