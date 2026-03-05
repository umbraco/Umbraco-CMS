import { elementFolderItemCache } from './element-folder-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { FolderItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiElementFolderItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<FolderItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: elementFolderItemCache,
			eventSources: ['Umbraco:CMS:ElementFolder'],
		});
	}
}
