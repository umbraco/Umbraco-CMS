import { UmbDataTypeItemServerDataSource } from './data-type-item.server.data-source.js';
import { UMB_DATA_TYPE_ITEM_STORE_CONTEXT } from './data-type-item.store.context-token.js';
import type { UmbDataTypeItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDataTypeItemRepository extends UmbItemRepositoryBase<UmbDataTypeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeItemServerDataSource, UMB_DATA_TYPE_ITEM_STORE_CONTEXT);
	}
}

export { UmbDataTypeItemRepository as api };
