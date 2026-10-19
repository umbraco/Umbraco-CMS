import { UmbManagementApiElementFolderItemDataCacheInvalidationManager } from './folder/repository/item/element-folder-item.server.cache-invalidation.manager.js';
import type { UmbEntryPointOnInit, UmbEntryPointOnUnload } from '@umbraco-cms/backoffice/extension-api';

import './global-components/index.js';

let folderItemDataCacheInvalidationManager: UmbManagementApiElementFolderItemDataCacheInvalidationManager | undefined;

export const onInit: UmbEntryPointOnInit = (host) => {
	folderItemDataCacheInvalidationManager = new UmbManagementApiElementFolderItemDataCacheInvalidationManager(host);
};

export const onUnload: UmbEntryPointOnUnload = () => {
	folderItemDataCacheInvalidationManager?.destroy();
};
