import { UmbUserItemServerDataSource } from './user-item.server.data-source.js';
import { UMB_USER_ITEM_STORE_CONTEXT } from './user-item.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UserItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbUserItemRepository extends UmbItemRepositoryBase<UserItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbUserItemServerDataSource, UMB_USER_ITEM_STORE_CONTEXT);
	}
}
