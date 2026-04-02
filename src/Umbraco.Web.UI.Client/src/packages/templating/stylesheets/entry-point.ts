import { UmbManagementApiStylesheetItemDataCacheInvalidationManager } from './repository/item/stylesheet-item.server.cache-invalidation.manager.js';
import type { UmbEntryPointOnInit, UmbEntryPointOnUnload } from '@umbraco-cms/backoffice/extension-api';

let itemDataCacheInvalidationManager: UmbManagementApiStylesheetItemDataCacheInvalidationManager | undefined;

export const onInit: UmbEntryPointOnInit = (host) => {
	itemDataCacheInvalidationManager = new UmbManagementApiStylesheetItemDataCacheInvalidationManager(host);
};

export const onUnload: UmbEntryPointOnUnload = () => {
	itemDataCacheInvalidationManager?.destroy();
};
