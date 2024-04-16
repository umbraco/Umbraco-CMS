import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		name: 'Document Search Provider',
		alias: 'Umb.SearchProvider.Document',
		type: 'searchProvider',
		api: () => import('./document.search-provider.js'),
		weight: 700,
		meta: {
			label: 'Documents',
		},
	},
	{
		name: 'Document Search Result Item ',
		alias: 'Umb.SearchResultItem.Document',
		type: 'searchResultItem',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
	},
];
