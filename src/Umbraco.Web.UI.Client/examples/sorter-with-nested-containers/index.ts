import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';

export const manifests: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		name: 'Example Sorter Dashboard',
		alias: 'example.dashboard.dataset',
		element: () => import('./sorter-dashboard.js'),
		weight: 900,
		meta: {
			label: 'Sorter example',
			pathname: 'sorter-example',
		},
	},
];
