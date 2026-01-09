import { UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS, UMB_MEMBER_GROUP_STORE_ALIAS } from './constants.js';
import { UmbManagementApiMemberGroupItemDataCacheInvalidationManager } from './member-group-item.server.cache-invalidation.manager.js';
import { UmbMemberGroupItemStore } from './member-group-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS,
		name: 'Member Group Item Repository',
		api: () => import('./member-group-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_MEMBER_GROUP_STORE_ALIAS,
		name: 'Member Group Item Store',
		api: UmbMemberGroupItemStore,
	},
	{
		name: 'Member Group Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.MemberGroup',
		type: 'globalContext',
		api: UmbManagementApiMemberGroupItemDataCacheInvalidationManager,
	},
];
