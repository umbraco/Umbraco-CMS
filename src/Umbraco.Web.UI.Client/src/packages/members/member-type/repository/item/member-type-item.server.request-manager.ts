/* eslint-disable local-rules/no-direct-api-import */
import { memberTypeItemCache } from './member-type-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MemberTypeService, type MemberTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiItemDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiMemberTypeItemDataRequestManager extends UmbManagementApiItemDataRequestManager<MemberTypeItemResponseModel> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<MemberTypeItemResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => MemberTypeService.getItemMemberType({ query: { id: ids } }),
			dataCache: memberTypeItemCache,
			inflightRequestCache: UmbManagementApiMemberTypeItemDataRequestManager.#inflightRequestCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
