import { UmbConfigureMfaProvidersApi } from './configure-mfa-providers-action.js';
import type { ManifestCurrentUserActionDefaultKind } from '@umbraco-cms/backoffice/extension-registry';

export const userProfileApps: Array<ManifestCurrentUserActionDefaultKind> = [
	{
		type: 'currentUserAction',
		kind: 'default',
		alias: 'Umb.CurrentUser.App.MfaLoginProviders',
		name: 'MFA Login Providers Current User App',
		weight: 800,
		api: UmbConfigureMfaProvidersApi,
		meta: {
			label: '#user_configureTwoFactor',
			icon: 'icon-rectangle-ellipsis',
			look: 'secondary',
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.AllowMfaAction',
			},
		],
	},
];
export const manifests = [...userProfileApps];
