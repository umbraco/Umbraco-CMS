import type { ManifestHeaderApp, ManifestUserDashboard } from '@umbraco-cms/models';

export const userDashboards: Array<ManifestUserDashboard> = [
	{
		type: 'user-dashboard',
		alias: 'Umb.UserDashboard.Test',
		name: 'Test User Dashboard',
		loader: () => import('./user-dashboard-test.element'),
		weight: 2,
		meta: {
			label: 'Test User Dashboard',
			pathname: 'test/test/test',
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

export const manifests = [...userDashboards, ...headerApps];
