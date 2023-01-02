import type { ManifestUserDashboard } from '@umbraco-cms/models';

export const manifests: Array<ManifestUserDashboard> = [
	{
		type: 'user-dashboard',
		alias: 'Umb.UserDashboard.Test',
		name: 'Test User Dashboard',
		elementName: 'umb-user-dashboard-test',
		loader: () => import('./user-dashboard-test.element'),
		weight: 2,
		meta: {
			label: 'Test User Dashboard',
			pathname: 'test/test/test',
		},
	},
];
