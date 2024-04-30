import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		name: 'Document Type Search Provider',
		alias: 'Umb.SearchProvider.DocumentType',
		type: 'searchProvider',
		api: () => import('./document-type.search-provider.js'),
		weight: 600,
		meta: {
			label: 'Document Types',
		},
	},
	{
		name: 'Document Type Search Result Item ',
		alias: 'Umb.SearchResultItem.DocumentType',
		type: 'searchResultItem',
		forEntityTypes: [UMB_DOCUMENT_TYPE_ENTITY_TYPE],
	},
];
