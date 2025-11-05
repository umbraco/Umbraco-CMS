/* eslint-disable local-rules/no-direct-api-import */
import { userGroupItemCache } from './user-group-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UserGroupService, type UserGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiUserGroupItemDataRequestManager extends UmbManagementApiItemDataRequestManager<UserGroupItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => UserGroupService.getItemUserGroup({ query: { id: ids } }),
			dataCache: userGroupItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
