import { UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_STORE_ALIAS } from './constants.js';
import { UmbDocumentItemStore } from './document-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_ITEM_REPOSITORY_ALIAS,
		name: 'Document Item Repository',
		api: () => import('./document-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_DOCUMENT_STORE_ALIAS,
		name: 'Document Item Store',
		api: UmbDocumentItemStore,
	},
];
