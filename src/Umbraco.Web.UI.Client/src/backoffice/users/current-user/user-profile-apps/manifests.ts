import type { ManifestUserProfileApp } from 'libs/extensions-registry/models';

export const userProfileApps: Array<ManifestUserProfileApp> = [
	{
		type: 'userProfileApp',
		alias: 'Umb.UserProfileApp.Themes',
		name: 'Themes User Profile App',
		loader: () => import('./user-profile-app-themes.element'),
		weight: 1,
		meta: {
			label: 'Themes User Profile App',
			pathname: 'themes',
		},
	},
];
export const manifests = [...userProfileApps];
