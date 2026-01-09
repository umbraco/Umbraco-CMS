import { UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_TYPE_ITEM_STORE_ALIAS } from './constants.js';
import { UmbManagementApiDocumentTypeItemDataCacheInvalidationManager } from './document-type-item.server.cache-invalidation.manager.js';
import { UmbDocumentTypeItemStore } from './document-type-item.store.js';

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
		api: UmbDocumentTypeItemStore,
	},
	{
		name: 'Document Type Item Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.DocumentType.Item',
		type: 'globalContext',
		api: UmbManagementApiDocumentTypeItemDataCacheInvalidationManager,
	},
];
