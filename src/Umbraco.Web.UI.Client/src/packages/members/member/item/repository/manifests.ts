import { UmbManagementApiMemberItemDataCacheInvalidationManager } from './member-item.server.cache-invalidation.manager.js';
import { UmbMemberItemStore } from './member-item.store.js';

export const UMB_MEMBER_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.MemberItem';
export const UMB_MEMBER_STORE_ALIAS = 'Umb.Store.MemberItem';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_ITEM_REPOSITORY_ALIAS,
		name: 'Member Item Repository',
		api: () => import('./member-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_MEMBER_STORE_ALIAS,
		name: 'Member Item Store',
		api: UmbMemberItemStore,
	},
	{
		name: 'Member Backoffice Cache Invalidation Manager',
		alias: 'Umb.EntryPoint.Member',
		type: 'globalContext',
		api: UmbManagementApiMemberItemDataCacheInvalidationManager,
	},
];
