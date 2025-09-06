import { UmbManagementApiWebhookItemDataCacheInvalidationManager } from './webhook/repository/item/webhook-item.server.cache-invalidation.manager.js';
import type { UmbEntryPointOnInit, UmbEntryPointOnUnload } from '@umbraco-cms/backoffice/extension-api';

let itemDataCacheInvalidationManager: UmbManagementApiWebhookItemDataCacheInvalidationManager | undefined;

export const onInit: UmbEntryPointOnInit = (host) => {
	itemDataCacheInvalidationManager = new UmbManagementApiWebhookItemDataCacheInvalidationManager(host);
};

export const onUnload: UmbEntryPointOnUnload = () => {
	itemDataCacheInvalidationManager?.destroy();
};
