/* eslint-disable local-rules/no-direct-api-import */
import { memberTypeItemCache } from './member-type-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MemberTypeService, type MemberTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiMemberTypeItemDataRequestManager extends UmbManagementApiItemDataRequestManager<MemberTypeItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => MemberTypeService.getItemMemberType({ query: { id: ids } }),
			dataCache: memberTypeItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
