/* eslint-disable local-rules/no-direct-api-import */
import { dataTypeDetailCache } from './data-type-detail.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	DataTypeService,
	type CreateDataTypeRequestModel,
	type DataTypeResponseModel,
	type UpdateDataTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiDetailDataRequestManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbApiResponse } from '@umbraco-cms/backoffice/resources';

export class UmbManagementApiDataTypeDetailDataRequestManager extends UmbManagementApiDetailDataRequestManager<
	DataTypeResponseModel,
	UpdateDataTypeRequestModel,
	CreateDataTypeRequestModel
> {
	static #inflightRequestCache = new Map<string, Promise<UmbApiResponse<{ data?: DataTypeResponseModel }>>>();

	constructor(host: UmbControllerHost) {
		super(host, {
			create: (body: CreateDataTypeRequestModel) => DataTypeService.postDataType({ body }),
			read: (id: string) => DataTypeService.getDataTypeById({ path: { id } }),
			update: (id: string, body: UpdateDataTypeRequestModel) => DataTypeService.putDataTypeById({ path: { id }, body }),
			delete: (id: string) => DataTypeService.deleteDataTypeById({ path: { id } }),
			dataCache: dataTypeDetailCache,
			inflightRequestCache: UmbManagementApiDataTypeDetailDataRequestManager.#inflightRequestCache,
		});
	}
}
