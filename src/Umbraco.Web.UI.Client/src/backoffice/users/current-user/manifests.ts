import type { ManifestTypes, ManifestUserProfileApp } from '@umbraco-cms/backoffice/extensions-registry';
import { manifests as modalManifests } from './modals/manifests';

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

export const headerApps: Array<ManifestTypes> = [
	{
		type: 'headerApp',
		alias: 'Umb.HeaderApp.CurrentUser',
		name: 'Current User',
		loader: () => import('./current-user-header-app.element'),
		weight: 1000,
		meta: {
			label: 'TODO: how should we enable this to not be set.',
			icon: 'TODO: how should we enable this to not be set.',
			pathname: 'user',
		},
	},
];

export const manifests = [...userProfileApps, ...headerApps, ...modalManifests];
