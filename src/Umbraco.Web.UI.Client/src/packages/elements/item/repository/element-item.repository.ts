import { UmbElementItemServerDataSource } from './element-item.server.data-source.js';
import { UMB_ELEMENT_ITEM_STORE_CONTEXT } from './element-item.store.context-token.js';
import type { UmbElementItemModel } from './types.js';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbElementItemRepository extends UmbItemRepositoryBase<UmbElementItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbElementItemServerDataSource, UMB_ELEMENT_ITEM_STORE_CONTEXT);
	}
}

export { UmbElementItemRepository as api };
