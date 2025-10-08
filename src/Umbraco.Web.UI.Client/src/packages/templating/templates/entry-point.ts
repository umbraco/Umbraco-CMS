import { UmbManagementApiTemplateItemDataCacheInvalidationManager } from './repository/item/template-item.server.cache-invalidation.manager.js';
import type { UmbEntryPointOnInit, UmbEntryPointOnUnload } from '@umbraco-cms/backoffice/extension-api';

let itemDataCacheInvalidationManager: UmbManagementApiTemplateItemDataCacheInvalidationManager | undefined;

export const onInit: UmbEntryPointOnInit = (host) => {
	itemDataCacheInvalidationManager = new UmbManagementApiTemplateItemDataCacheInvalidationManager(host);
};

export const onUnload: UmbEntryPointOnUnload = () => {
	itemDataCacheInvalidationManager?.destroy();
};
