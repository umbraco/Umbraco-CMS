/* eslint-disable local-rules/no-direct-api-import */
import { dataTypeItemCache } from './data-type-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DataTypeService, type DataTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiDataTypeItemDataRequestManager extends UmbManagementApiItemDataRequestManager<DataTypeItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => DataTypeService.getItemDataType({ query: { id: ids } }),
			dataCache: dataTypeItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
