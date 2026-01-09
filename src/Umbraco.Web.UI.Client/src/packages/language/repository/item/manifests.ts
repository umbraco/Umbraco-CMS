import { UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS, UMB_LANGUAGE_STORE_ALIAS } from './constants.js';
import { UmbManagementApiLanguageItemDataCacheInvalidationManager } from './language-item.server.cache-invalidation.manager.js';
import { UmbLanguageItemStore } from './language-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS,
		name: 'Language Item Repository',
		api: () => import('./language-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_LANGUAGE_STORE_ALIAS,
		name: 'Language Item Store',
		api: UmbLanguageItemStore,
	},
	{
		name: 'Language Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.Language',
		type: 'globalContext',
		api: UmbManagementApiLanguageItemDataCacheInvalidationManager,
	},
];
