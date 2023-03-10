import { manifests as modalManifests } from './modals/manifests';
import type { ManifestHeaderApp, ManifestUserDashboard } from '@umbraco-cms/models';

export const userDashboards: Array<ManifestUserDashboard> = [
	{
		type: 'userDashboard',
		alias: 'Umb.UserDashboard.Themes',
		name: 'Themes User Dashboard',
		loader: () => import('./user-dashboard-themes.element'),
		weight: 1,
		meta: {
			label: 'Themes User Dashboard',
			pathname: 'themes',
		},
	},
];

export const headerApps: Array<ManifestHeaderApp> = [
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

export const manifests = [...userDashboards, ...headerApps, ...modalManifests];
