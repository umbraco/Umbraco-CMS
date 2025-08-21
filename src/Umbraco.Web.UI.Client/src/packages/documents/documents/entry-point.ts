import { UmbManagementApiDocumentItemDataCacheInvalidationManager } from './item/repository/document-item.server.cache-invalidation.manager.js';
import type { UmbEntryPointOnInit, UmbEntryPointOnUnload } from '@umbraco-cms/backoffice/extension-api';

let itemDataCacheInvalidationManager: UmbManagementApiDocumentItemDataCacheInvalidationManager | undefined;

export const onInit: UmbEntryPointOnInit = (host) => {
	itemDataCacheInvalidationManager = new UmbManagementApiDocumentItemDataCacheInvalidationManager(host);
};

export const onUnload: UmbEntryPointOnUnload = () => {
	itemDataCacheInvalidationManager?.destroy();
};
