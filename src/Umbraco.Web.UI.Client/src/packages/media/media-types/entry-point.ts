import { UmbManagementApiMediaTypeDetailDataCacheInvalidationManager } from './repository/detail/server-data-source/media-type-detail.server.cache-invalidation.manager.js';
import { UmbManagementApiMediaTypeItemDataCacheInvalidationManager } from './repository/item/media-type-item.server.cache-invalidation.manager.js';
import type { UmbEntryPointOnInit, UmbEntryPointOnUnload } from '@umbraco-cms/backoffice/extension-api';

let detailDataCacheInvalidationManager: UmbManagementApiMediaTypeDetailDataCacheInvalidationManager | undefined;
let itemDataCacheInvalidationManager: UmbManagementApiMediaTypeItemDataCacheInvalidationManager | undefined;

export const onInit: UmbEntryPointOnInit = (host) => {
	detailDataCacheInvalidationManager = new UmbManagementApiMediaTypeDetailDataCacheInvalidationManager(host);
	itemDataCacheInvalidationManager = new UmbManagementApiMediaTypeItemDataCacheInvalidationManager(host);
};

export const onUnload: UmbEntryPointOnUnload = () => {
	detailDataCacheInvalidationManager?.destroy();
	itemDataCacheInvalidationManager?.destroy();
};
