import type { ManifestUserProfileApp } from 'libs/extensions-registry/models';

export const userProfileApps: Array<ManifestUserProfileApp> = [
	{
		type: 'userProfileApp',
		alias: 'Umb.UserProfileApp.Themes',
		name: 'Themes User Profile App',
		loader: () => import('./user-profile-app-themes.element'),
		weight: 100,
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
		weight: 200,
		meta: {
			label: 'History User Profile App',
			pathname: 'history',
		},
	},
];
export const manifests = [...userProfileApps];
