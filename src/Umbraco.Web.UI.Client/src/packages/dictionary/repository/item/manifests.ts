import { UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS, UMB_DICTIONARY_STORE_ALIAS } from './constants.js';
import { UmbManagementApiDictionaryItemDataCacheInvalidationManager } from './dictionary-item.server.cache-invalidation.manager.js';
import { UmbDictionaryItemStore } from './dictionary-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS,
		name: 'Dictionary Item Repository',
		api: () => import('./dictionary-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_DICTIONARY_STORE_ALIAS,
		name: 'Dictionary Item Store',
		api: UmbDictionaryItemStore,
	},
	{
		name: 'Dictionary Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.Dictionary',
		type: 'globalContext',
		api: UmbManagementApiDictionaryItemDataCacheInvalidationManager,
	},
];
