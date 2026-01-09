import { UMB_DOCUMENT_BLUEPRINT_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_BLUEPRINT_STORE_ALIAS } from './constants.js';
import { UmbManagementApiDocumentBlueprintItemDataCacheInvalidationManager } from './document-blueprint-item.server.cache-invalidation.manager.js';
import { UmbDocumentBlueprintItemStore } from './document-blueprint-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_BLUEPRINT_ITEM_REPOSITORY_ALIAS,
		name: 'Document Blueprint Item Repository',
		api: () => import('./document-blueprint-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_DOCUMENT_BLUEPRINT_STORE_ALIAS,
		name: 'Document Blueprint Item Store',
		api: UmbDocumentBlueprintItemStore,
	},
	{
		name: 'Document Blueprint Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.DocumentBlueprint',
		type: 'globalContext',
		api: UmbManagementApiDocumentBlueprintItemDataCacheInvalidationManager,
	},
];
