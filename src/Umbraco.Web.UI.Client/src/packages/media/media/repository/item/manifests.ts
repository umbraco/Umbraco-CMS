import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_STORE_ALIAS } from './constants.js';
import { UmbManagementApiMediaItemDataCacheInvalidationManager } from './media-item.server.cache-invalidation.manager.js';
import { UmbMediaItemStore } from './media-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_ITEM_REPOSITORY_ALIAS,
		name: 'Media Item Repository',
		api: () => import('./media-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_MEDIA_STORE_ALIAS,
		name: 'Media Item Store',
		api: UmbMediaItemStore,
	},
	{
		name: 'Media Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.Media',
		type: 'globalContext',
		api: UmbManagementApiMediaItemDataCacheInvalidationManager,
	},
];
