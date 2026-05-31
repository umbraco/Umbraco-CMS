import { UmbManagementApiMemberTypeDetailDataCacheInvalidationManager } from './repository/detail/server-data-source/member-type-detail.server.cache-invalidation.manager.js';
import { UmbManagementApiMemberTypeItemDataCacheInvalidationManager } from './repository/item/member-type-item.server.cache-invalidation.manager.js';
import type { UmbEntryPointOnInit, UmbEntryPointOnUnload } from '@umbraco-cms/backoffice/extension-api';

let detailDataCacheInvalidationManager: UmbManagementApiMemberTypeDetailDataCacheInvalidationManager | undefined;
let itemDataCacheInvalidationManager: UmbManagementApiMemberTypeItemDataCacheInvalidationManager | undefined;

export const onInit: UmbEntryPointOnInit = (host) => {
	detailDataCacheInvalidationManager = new UmbManagementApiMemberTypeDetailDataCacheInvalidationManager(host);
	itemDataCacheInvalidationManager = new UmbManagementApiMemberTypeItemDataCacheInvalidationManager(host);
};

export const onUnload: UmbEntryPointOnUnload = () => {
	detailDataCacheInvalidationManager?.destroy();
	itemDataCacheInvalidationManager?.destroy();
};
