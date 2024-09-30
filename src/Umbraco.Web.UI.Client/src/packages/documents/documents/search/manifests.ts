import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Document Search Provider',
		alias: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
		type: 'searchProvider',
		api: () => import('./document.search-provider.js'),
		weight: 800,
		meta: {
			label: 'Documents',
		},
	},
	{
		name: 'Document Search Result Item ',
		alias: 'Umb.SearchResultItem.Document',
		type: 'searchResultItem',
		element: () => import('./document-search-result-item.element.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
	},
];
