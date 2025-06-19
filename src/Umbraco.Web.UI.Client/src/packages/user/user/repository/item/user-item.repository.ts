import { UmbUserItemServerDataSource } from './user-item.server.data-source.js';
import { UMB_USER_ITEM_STORE_CONTEXT } from './user-item.store.token.js';
import type { UmbUserItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbUserItemRepository extends UmbItemRepositoryBase<UmbUserItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbUserItemServerDataSource, UMB_USER_ITEM_STORE_CONTEXT);
	}
}

export default UmbUserItemRepository;
