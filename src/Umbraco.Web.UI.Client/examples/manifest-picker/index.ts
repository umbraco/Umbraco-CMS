import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';

export const manifests: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		name: 'Example Manifest Picker Dashboard',
		alias: 'example.dashboard.manifestPicker',
		element: () => import('./manifest-picker-dashboard.js'),
		weight: 1000,
		meta: {
			label: 'Manifest Picker example',
			pathname: 'example-manifest-picker',
		},
	},
];
