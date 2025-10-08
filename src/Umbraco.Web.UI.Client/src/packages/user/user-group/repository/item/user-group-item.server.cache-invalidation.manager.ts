import { userGroupItemCache } from './user-group-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UserGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiUserGroupItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<UserGroupItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: userGroupItemCache,
			eventSources: ['Umbraco:CMS:UserGroup'],
		});
	}
}
