import { UMB_SCRIPT_ITEM_REPOSITORY_ALIAS } from './constants.js';
import { UmbManagementApiScriptItemDataCacheInvalidationManager } from './script-item.server.cache-invalidation.manager.js';
import { UmbScriptItemStore } from './script-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_SCRIPT_ITEM_REPOSITORY_ALIAS,
		name: 'Script Item Repository',
		api: () => import('./script-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: 'Umb.ItemStore.Script',
		name: 'Script Item Store',
		api: UmbScriptItemStore,
	},
	{
		name: 'Script Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.Script',
		type: 'globalContext',
		api: UmbManagementApiScriptItemDataCacheInvalidationManager,
	},
];
