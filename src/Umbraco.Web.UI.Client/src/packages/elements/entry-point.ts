import { UmbManagementApiElementDetailDataCacheInvalidationManager } from './repository/detail/element-detail.server.cache-invalidation.manager.js';
import { UmbManagementApiElementItemDataCacheInvalidationManager } from './item/repository/element-item.server.cache-invalidation.manager.js';
import type { UmbEntryPointOnInit, UmbEntryPointOnUnload } from '@umbraco-cms/backoffice/extension-api';

import './global-components/index.js';

let detailDataCacheInvalidationManager: UmbManagementApiElementDetailDataCacheInvalidationManager | undefined;
let itemDataCacheInvalidationManager: UmbManagementApiElementItemDataCacheInvalidationManager | undefined;

export const onInit: UmbEntryPointOnInit = (host) => {
	detailDataCacheInvalidationManager = new UmbManagementApiElementDetailDataCacheInvalidationManager(host);
	itemDataCacheInvalidationManager = new UmbManagementApiElementItemDataCacheInvalidationManager(host);
};

export const onUnload: UmbEntryPointOnUnload = () => {
	detailDataCacheInvalidationManager?.destroy();
	itemDataCacheInvalidationManager?.destroy();
};
