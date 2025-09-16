import { UmbManagementApiDocumentTypeDetailDataCacheInvalidationManager } from './repository/detail/server-data-source/document-type-detail.server.cache-invalidation.manager.js';
import { UmbManagementApiDocumentTypeItemDataCacheInvalidationManager } from './repository/item/document-type-item.server.cache-invalidation.manager.js';
import type { UmbEntryPointOnInit, UmbEntryPointOnUnload } from '@umbraco-cms/backoffice/extension-api';

let detailDataCacheInvalidationManager: UmbManagementApiDocumentTypeDetailDataCacheInvalidationManager | undefined;
let itemDataCacheInvalidationManager: UmbManagementApiDocumentTypeItemDataCacheInvalidationManager | undefined;

export const onInit: UmbEntryPointOnInit = (host) => {
	detailDataCacheInvalidationManager = new UmbManagementApiDocumentTypeDetailDataCacheInvalidationManager(host);
	itemDataCacheInvalidationManager = new UmbManagementApiDocumentTypeItemDataCacheInvalidationManager(host);
};

export const onUnload: UmbEntryPointOnUnload = () => {
	detailDataCacheInvalidationManager?.destroy();
	itemDataCacheInvalidationManager?.destroy();
};
