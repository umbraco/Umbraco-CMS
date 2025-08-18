import { dataTypeDetailCache } from './data-type-detail.server.cache.js';
import { UmbManagementApiDetailDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DataTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiDataTypeDetailDataCacheInvalidationManager extends UmbManagementApiDetailDataCacheInvalidationManager<DataTypeResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: dataTypeDetailCache,
			eventSources: ['Umbraco:CMS:DataType'],
		});
	}
}
