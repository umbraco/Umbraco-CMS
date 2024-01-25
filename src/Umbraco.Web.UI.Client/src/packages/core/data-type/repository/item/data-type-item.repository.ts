import { UmbDataTypeItemServerDataSource } from './data-type-item.server.data.js';
import { UMB_DATA_TYPE_ITEM_STORE_CONTEXT } from './data-type-item.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { DataTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDataTypeItemRepository extends UmbItemRepositoryBase<DataTypeItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDataTypeItemServerDataSource, UMB_DATA_TYPE_ITEM_STORE_CONTEXT);
	}
}
