import { memberTypeDetailCache } from './member-type-detail.server.cache.js';
import { UmbManagementApiDetailDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { MemberTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiMemberTypeDetailDataCacheInvalidationManager extends UmbManagementApiDetailDataCacheInvalidationManager<MemberTypeResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: memberTypeDetailCache,
			eventSources: ['Umbraco:CMS:MemberType'],
		});
	}
}
