import { UMB_USER_ALLOW_CHANGE_PASSWORD_CONDITION_ALIAS, UMB_USER_ENTITY_TYPE } from '@umbraco-cms/backoffice/user';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.User.ChangePassword',
		name: 'Change User Password Entity Action',
		weight: 600,
		api: () => import('./change-user-password.action.js'),
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
		meta: {
			icon: 'icon-key',
			label: '#user_changePassword',
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.IsDefaultKind',
			},
			{
				alias: UMB_USER_ALLOW_CHANGE_PASSWORD_CONDITION_ALIAS,
			},
		],
	},
];
