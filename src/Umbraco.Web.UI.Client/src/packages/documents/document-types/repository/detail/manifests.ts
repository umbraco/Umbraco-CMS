import { UMB_DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_DOCUMENT_TYPE_DETAIL_STORE_ALIAS } from './constants.js';
import { UmbManagementApiDocumentTypeDetailDataCacheInvalidationManager } from './server-data-source/document-type-detail.server.cache-invalidation.manager.js';
import { UmbDocumentTypeDetailStore } from './document-type-detail.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
		name: 'Document Types Repository',
		api: () => import('./document-type-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_DOCUMENT_TYPE_DETAIL_STORE_ALIAS,
		name: 'Document Type Store',
		api: UmbDocumentTypeDetailStore,
	},
	{
		name: 'Document Type Detail Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.DocumentType.Detail',
		type: 'globalContext',
		api: UmbManagementApiDocumentTypeDetailDataCacheInvalidationManager,
	},
];
