import { UMB_CURRENT_USER_ALLOW_MFA_CONDITION_ALIAS } from '@umbraco-cms/backoffice/user';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'currentUserAction',
		kind: 'default',
		alias: 'Umb.CurrentUser.App.MfaLoginProviders',
		name: 'MFA Login Providers Current User App',
		weight: 800,
		api: () => import('./configure-mfa-providers-action.js'),
		meta: {
			label: '#user_configureTwoFactor',
			icon: 'icon-rectangle-ellipsis',
			look: 'secondary',
		},
		conditions: [
			{
				alias: UMB_CURRENT_USER_ALLOW_MFA_CONDITION_ALIAS,
			},
		],
	},
];
