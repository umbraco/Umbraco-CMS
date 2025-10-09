/* eslint-disable local-rules/no-direct-api-import */
import { memberItemCache } from './member-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MemberService, type MemberItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiMemberItemDataRequestManager extends UmbManagementApiItemDataRequestManager<MemberItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => MemberService.getItemMember({ query: { id: ids } }),
			dataCache: memberItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
