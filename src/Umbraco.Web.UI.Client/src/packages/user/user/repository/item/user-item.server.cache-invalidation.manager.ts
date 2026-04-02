import { userItemCache } from './user-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UserItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiUserItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<UserItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: userItemCache,
			eventSources: ['Umbraco:CMS:User'],
		});
	}
}
