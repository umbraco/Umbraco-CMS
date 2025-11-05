import { partialViewItemCache } from './partial-view-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { PartialViewItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiPartialViewItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<PartialViewItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: partialViewItemCache,
			eventSources: ['Umbraco:CMS:PartialView'],
		});
	}
}
