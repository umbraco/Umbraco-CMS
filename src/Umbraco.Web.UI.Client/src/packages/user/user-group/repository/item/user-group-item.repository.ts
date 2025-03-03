import { UmbUserGroupItemServerDataSource } from './user-group-item.server.data-source.js';
import { UMB_USER_GROUP_ITEM_STORE_CONTEXT } from './user-group-item.store.token.js';
import type { UmbUserGroupItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbUserGroupItemRepository extends UmbItemRepositoryBase<UmbUserGroupItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbUserGroupItemServerDataSource, UMB_USER_GROUP_ITEM_STORE_CONTEXT);
	}
}

export default UmbUserGroupItemRepository;
