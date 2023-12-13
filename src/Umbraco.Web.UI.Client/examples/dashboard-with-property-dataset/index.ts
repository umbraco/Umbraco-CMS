import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'dashboard',
		name: 'Example Dataset Workspace View',
		alias: 'example.dashboard.dataset',
		element: () => import('./dataset-dashboard.js'),
		weight: 900,
		meta: {
			label: 'Dataset example',
			pathname: 'dataset-example',
		},
	},
];
