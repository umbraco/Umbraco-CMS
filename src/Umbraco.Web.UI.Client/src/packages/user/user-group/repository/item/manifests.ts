import { UMB_USER_GROUP_ITEM_REPOSITORY_ALIAS, UMB_USER_GROUP_STORE_ALIAS } from './constants.js';
import { UmbManagementApiUserGroupItemDataCacheInvalidationManager } from './user-group-item.server.cache-invalidation.manager.js';
import { UmbUserGroupItemStore } from './user-group-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_USER_GROUP_ITEM_REPOSITORY_ALIAS,
		name: 'User Group Item Repository',
		api: () => import('./user-group-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_USER_GROUP_STORE_ALIAS,
		name: 'User Group Item Store',
		api: UmbUserGroupItemStore,
	},
	{
		name: 'User Group Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.UserGroup',
		type: 'globalContext',
		api: UmbManagementApiUserGroupItemDataCacheInvalidationManager,
	},
];
