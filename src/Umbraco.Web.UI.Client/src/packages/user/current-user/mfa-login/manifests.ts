import type { ManifestUserProfileApp } from '@umbraco-cms/backoffice/extension-registry';

export const userProfileApps: Array<ManifestUserProfileApp> = [
	{
		type: 'userProfileApp',
		alias: 'Umb.UserProfileApp.CurrentUser.MfaLoginProviders',
		name: 'MFA Login Providers User Profile App',
		element: () => import('./mfa-providers-user-profile-app.element.js'),
		weight: 800,
		meta: {
			label: 'Two-Factor Authentication',
			pathname: 'mfa-providers',
		},
	},
];
export const manifests = [...userProfileApps];
