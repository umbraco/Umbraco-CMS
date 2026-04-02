/* eslint-disable local-rules/no-direct-api-import */
import { userGroupItemCache } from './user-group-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UserGroupService, type UserGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiItemDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiUserGroupItemDataRequestManager extends UmbManagementApiItemDataRequestManager<UserGroupItemResponseModel> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<UserGroupItemResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => UserGroupService.getItemUserGroup({ query: { id: ids } }),
			dataCache: userGroupItemCache,
			inflightRequestCache: UmbManagementApiUserGroupItemDataRequestManager.#inflightRequestCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
