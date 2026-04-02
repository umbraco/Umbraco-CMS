import { UmbManagementApiMediaItemDataCacheInvalidationManager } from './repository/item/media-item.server.cache-invalidation.manager.js';
import type { UmbEntryPointOnInit, UmbEntryPointOnUnload } from '@umbraco-cms/backoffice/extension-api';

let itemDataCacheInvalidationManager: UmbManagementApiMediaItemDataCacheInvalidationManager | undefined;

export const onInit: UmbEntryPointOnInit = (host) => {
	itemDataCacheInvalidationManager = new UmbManagementApiMediaItemDataCacheInvalidationManager(host);
};

export const onUnload: UmbEntryPointOnUnload = () => {
	itemDataCacheInvalidationManager?.destroy();
};
