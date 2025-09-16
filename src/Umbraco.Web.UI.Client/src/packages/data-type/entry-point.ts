import { UmbManagementApiDataTypeDetailDataCacheInvalidationManager } from './repository/detail/server-data-source/data-type-detail.server.cache-invalidation.manager.js';
import { UmbManagementApiDataTypeItemDataCacheInvalidationManager } from './repository/item/data-type-item.server.cache-invalidation.manager.js';
import type { UmbEntryPointOnInit, UmbEntryPointOnUnload } from '@umbraco-cms/backoffice/extension-api';

let detailDataCacheInvalidationManager: UmbManagementApiDataTypeDetailDataCacheInvalidationManager | undefined;
let itemDataCacheInvalidationManager: UmbManagementApiDataTypeItemDataCacheInvalidationManager | undefined;

export const onInit: UmbEntryPointOnInit = (host) => {
	detailDataCacheInvalidationManager = new UmbManagementApiDataTypeDetailDataCacheInvalidationManager(host);
	itemDataCacheInvalidationManager = new UmbManagementApiDataTypeItemDataCacheInvalidationManager(host);
};

export const onUnload: UmbEntryPointOnUnload = () => {
	detailDataCacheInvalidationManager?.destroy();
	itemDataCacheInvalidationManager?.destroy();
};
