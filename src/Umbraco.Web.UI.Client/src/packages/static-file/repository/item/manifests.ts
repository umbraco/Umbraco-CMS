import { UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS, UMB_STATIC_FILE_STORE_ALIAS } from './constants.js';
import { UmbManagementApiStaticFileItemDataCacheInvalidationManager } from './static-file-item.server.cache-invalidation.manager.js';
import { UmbStaticFileItemStore } from './static-file-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS,
		name: 'Static File Item Repository',
		api: () => import('./static-file-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_STATIC_FILE_STORE_ALIAS,
		name: 'Static File Item Store',
		api: UmbStaticFileItemStore,
	},
	{
		name: 'Static File Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.StaticFile',
		type: 'globalContext',
		api: UmbManagementApiStaticFileItemDataCacheInvalidationManager,
	},
];
