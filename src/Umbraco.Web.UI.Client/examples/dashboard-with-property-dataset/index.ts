import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';

export const manifests: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		name: 'Example Dataset Dashboard',
		alias: 'example.dashboard.dataset',
		element: () => import('./dataset-dashboard.js'),
		weight: 900,
		meta: {
			label: 'Dataset example',
			pathname: 'dataset-example',
		},
	},
];
