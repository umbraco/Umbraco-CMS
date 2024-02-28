import type { ManifestUserProfileApp } from '@umbraco-cms/backoffice/extension-registry';

export const userProfileApps: Array<ManifestUserProfileApp> = [
	{
		type: 'userProfileApp',
		alias: 'Umb.UserProfileApp.CurrentUser.ExternalLoginProviders',
		name: 'External Login Providers User Profile App',
		element: () => import('./external-login-providers-user-profile-app.element.js'),
		weight: 800,
		meta: {
			label: 'External Login Providers User Profile App',
			pathname: 'externalLoginProviders',
		},
	},
];
export const manifests = [...userProfileApps];
