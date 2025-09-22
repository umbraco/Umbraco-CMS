import { memberTypeItemCache } from './member-type-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { MemberTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiMemberTypeItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<MemberTypeItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: memberTypeItemCache,
			eventSources: ['Umbraco:CMS:MemberType'],
		});
	}
}
