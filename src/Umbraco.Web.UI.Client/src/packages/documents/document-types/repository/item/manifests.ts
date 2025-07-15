import { UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_TYPE_ITEM_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
		name: 'Document Type Item Repository',
		api: () => import('./document-type-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_DOCUMENT_TYPE_ITEM_STORE_ALIAS,
		name: 'Document Type Item Store',
		api: () => import('./document-type-item.store.js'),
	},
];
