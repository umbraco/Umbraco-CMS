import type { ManifestUserProfileApp } from '@umbraco-cms/backoffice/extension-registry';

export const userProfileApps: Array<ManifestUserProfileApp> = [
	{
		type: 'userProfileApp',
		alias: 'Umb.UserProfileApp.CurrentUser.Profile',
		name: 'Current User Profile User Profile App',
		element: () => import('./current-user-profile-user-profile-app.element.js'),
		weight: 900,
		meta: {
			label: 'Current User Profile User Profile App',
			pathname: 'profile',
		},
	},
];
export const manifests = [...userProfileApps];
