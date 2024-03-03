import {
	UMB_CHANGE_USER_PASSWORD_REPOSITORY_ALIAS,
	UMB_UNLOCK_USER_REPOSITORY_ALIAS,
	UMB_USER_DETAIL_REPOSITORY_ALIAS,
	UMB_USER_ITEM_REPOSITORY_ALIAS,
} from '../repository/index.js';
import { UMB_USER_ENTITY_TYPE } from '../entity.js';
import { UmbDisableUserEntityAction } from './disable/disable-user.action.js';
import { UmbEnableUserEntityAction } from './enable/enable-user.action.js';
import { UmbChangeUserPasswordEntityAction } from './change-password/change-user-password.action.js';
import { UmbUnlockUserEntityAction } from './unlock/unlock-user.action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.User.Delete',
		name: 'Delete User Entity Action',
		kind: 'delete',
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_USER_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_USER_ITEM_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.AllowDeleteAction',
			},
		],
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.User.Enable',
		name: 'Enable User Entity Action',
		weight: 800,
		api: UmbEnableUserEntityAction,
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			icon: 'icon-check',
			label: 'Enable',
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.AllowEnableAction',
			},
		],
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.User.Disable',
		name: 'Disable User Entity Action',
		weight: 700,
		api: UmbDisableUserEntityAction,
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			icon: 'icon-block',
			label: 'Disable',
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.AllowDisableAction',
			},
		],
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.User.ChangePassword',
		name: 'Change User Password Entity Action',
		weight: 600,
		api: UmbChangeUserPasswordEntityAction,
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			icon: 'icon-key',
			label: 'Change Password',
			repositoryAlias: UMB_CHANGE_USER_PASSWORD_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.User.Unlock',
		name: 'Unlock User Entity Action',
		weight: 600,
		api: UmbUnlockUserEntityAction,
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			icon: 'icon-unlocked',
			label: 'Unlock',
			repositoryAlias: UMB_UNLOCK_USER_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.AllowUnlockAction',
			},
		],
	},
];

export const manifests = [...entityActions];
