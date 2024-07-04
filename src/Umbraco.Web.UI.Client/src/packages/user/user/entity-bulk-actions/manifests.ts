import { UMB_USER_COLLECTION_ALIAS } from '../collection/index.js';
import { UMB_USER_ENTITY_TYPE } from '../entity.js';
import { UmbEnableUserEntityBulkAction } from './enable/enable.action.js';
import { UmbSetGroupUserEntityBulkAction } from './set-group/set-group.action.js';
import { UmbUnlockUserEntityBulkAction } from './unlock/unlock.action.js';
import { UmbDisableUserEntityBulkAction } from './disable/disable.action.js';
import type { ManifestEntityBulkAction, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

const entityActions: Array<ManifestEntityBulkAction> = [
	/* TODO: Implement SetGroup entity action
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.User.SetGroup',
		name: 'SetGroup User Entity Bulk Action',
		weight: 400,
		api: UmbSetGroupUserEntityBulkAction,
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			label: 'SetGroup',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
			},
		],
	},
	*/
	{
		type: 'entityBulkAction',
		alias: 'Umb.EntityBulkAction.User.Enable',
		name: 'Enable User Entity Bulk Action',
		weight: 300,
		api: UmbEnableUserEntityBulkAction,
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			label: 'Enable',
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
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			label: 'Unlock',
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
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			label: 'Disable',
		},
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_USER_COLLECTION_ALIAS,
			},
		],
	},
];

export const manifests: Array<ManifestTypes> = [...entityActions];
