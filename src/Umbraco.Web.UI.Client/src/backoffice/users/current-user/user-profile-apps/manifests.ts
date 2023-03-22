import type { ManifestUserProfileApp } from 'libs/extensions-registry/models';

export const userProfileApps: Array<ManifestUserProfileApp> = [
	{
		type: 'userProfileApp',
		alias: 'Umb.UserProfileApp.ExternalLoginProviders',
		name: 'External Login Providers User Profile App',
		loader: () => import('./user-profile-app-external-login-providers.element'),
		weight: 100,
		meta: {
			label: 'External Login Providers User Profile App',
			pathname: 'externalLoginProviders',
		},
	},
	{
		type: 'userProfileApp',
		alias: 'Umb.UserProfileApp.Themes',
		name: 'Themes User Profile App',
		loader: () => import('./user-profile-app-themes.element'),
		weight: 200,
		meta: {
			label: 'Themes User Profile App',
			pathname: 'themes',
		},
	},
	{
		type: 'userProfileApp',
		alias: 'Umb.UserProfileApp.History',
		name: 'History User Profile App',
		loader: () => import('./user-profile-app-history.element'),
		weight: 300,
		meta: {
			label: 'History User Profile App',
			pathname: 'history',
		},
	},
];
export const manifests = [...userProfileApps];
