/* eslint-disable local-rules/no-direct-api-import */
import { dataTypeItemCache } from './data-type-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DataTypeService, type DataTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiItemDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiDataTypeItemDataRequestManager extends UmbManagementApiItemDataRequestManager<DataTypeItemResponseModel> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<DataTypeItemResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => DataTypeService.getItemDataType({ query: { id: ids } }),
			dataCache: dataTypeItemCache,
			inflightRequestCache: UmbManagementApiDataTypeItemDataRequestManager.#inflightRequestCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
