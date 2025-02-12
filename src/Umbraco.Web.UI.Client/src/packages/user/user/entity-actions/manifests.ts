import {
	UMB_USER_ALLOW_MFA_CONDITION_ALIAS,
	UMB_USER_DETAIL_REPOSITORY_ALIAS,
	UMB_USER_ITEM_REPOSITORY_ALIAS,
} from '../constants.js';
import { UMB_USER_ENTITY_TYPE } from '../entity.js';

import { manifests as createManifests } from './create/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.User.Delete',
		name: 'Delete User Entity Action',
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
		kind: 'default',
		alias: 'Umb.EntityAction.User.Enable',
		name: 'Enable User Entity Action',
		weight: 800,
		api: () => import('./enable/enable-user.action.js'),
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			icon: 'icon-check',
			label: '#actions_enable',
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.AllowEnableAction',
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.User.Disable',
		name: 'Disable User Entity Action',
		weight: 700,
		api: () => import('./disable/disable-user.action.js'),
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			icon: 'icon-block',
			label: '#actions_disable',
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.AllowDisableAction',
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.User.Unlock',
		name: 'Unlock User Entity Action',
		weight: 600,
		api: () => import('./unlock/unlock-user.action.js'),
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			icon: 'icon-unlocked',
			label: '#actions_unlock',
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.AllowUnlockAction',
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.User.ConfigureMfa',
		name: 'Configure MFA Entity Action',
		weight: 500,
		api: () => import('./mfa/mfa-user.action.js'),
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			icon: 'icon-settings',
			label: '#user_configureMfa',
			additionalOptions: true,
		},
		conditions: [
			{
				alias: UMB_USER_ALLOW_MFA_CONDITION_ALIAS,
			},
		],
	},
];

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...entityActions, ...createManifests];
