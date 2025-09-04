import { memberGroupItemCache } from './member-group-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { MemberGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiMemberGroupItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<MemberGroupItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: memberGroupItemCache,
			eventSources: ['Umbraco:CMS:MemberGroup'],
		});
	}
}
