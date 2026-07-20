/* eslint-disable local-rules/no-direct-api-import */
import { memberGroupItemCache } from './member-group-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MemberGroupService, type MemberGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiItemDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiMemberGroupItemDataRequestManager extends UmbManagementApiItemDataRequestManager<MemberGroupItemResponseModel> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<MemberGroupItemResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => MemberGroupService.getItemMemberGroup({ query: { id: ids } }),
			dataCache: memberGroupItemCache,
			inflightRequestCache: UmbManagementApiMemberGroupItemDataRequestManager.#inflightRequestCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
