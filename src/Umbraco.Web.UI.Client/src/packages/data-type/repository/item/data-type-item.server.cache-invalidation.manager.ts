import { dataTypeItemCache } from './data-type-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DataTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiDataTypeItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<DataTypeItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: dataTypeItemCache,
			eventSources: ['Umbraco:CMS:DataType'],
		});
	}
}
