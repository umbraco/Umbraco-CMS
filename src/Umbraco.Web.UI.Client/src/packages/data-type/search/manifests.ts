import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		name: 'Data Type Search Provider',
		alias: 'Umb.SearchProvider.DataType',
		type: 'searchProvider',
		api: () => import('./data-type.search-provider.js'),
		weight: 900,
		meta: {
			label: 'Data Types',
		},
	},
];
