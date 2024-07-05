import type { ManifestCurrentUserActionDefaultKind, ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CurrentUserExternalLogin',
		name: 'External Login Modal',
		js: () => import('./modals/external-login-modal.element.js'),
	},
];

export const userProfileApps: Array<ManifestCurrentUserActionDefaultKind> = [
	{
		type: 'currentUserAction',
		kind: 'default',
		alias: 'Umb.CurrentUser.App.ExternalLoginProviders',
		name: 'External Login Providers Current User App',
		weight: 700,
		api: () => import('./configure-external-login-providers-action.js'),
		meta: {
			label: '#defaultdialogs_externalLoginProviders',
			icon: 'icon-lock',
			look: 'secondary',
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.AllowExternalLoginAction',
			},
		],
	},
];
export const manifests = [...modals, ...userProfileApps];
