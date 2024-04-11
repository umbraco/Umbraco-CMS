import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		name: 'Document Search Provider',
		alias: 'Umb.SearchProvider.Document',
		type: 'searchProvider',
		api: () => import('./document.search-provider.js'),
		weight: 1000,
		meta: {
			label: 'Documents',
		},
	},
];
