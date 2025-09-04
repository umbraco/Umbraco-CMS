/* eslint-disable local-rules/no-direct-api-import */
import { userItemCache } from './user-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UserService, type UserItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiUserItemDataRequestManager extends UmbManagementApiItemDataRequestManager<UserItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => UserService.getItemUser({ query: { id: ids } }),
			dataCache: userItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
