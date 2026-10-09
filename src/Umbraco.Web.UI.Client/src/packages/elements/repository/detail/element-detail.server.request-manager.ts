/* eslint-disable local-rules/no-direct-api-import */
import { elementDetailCache } from './element-detail.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	ElementService,
	type CreateElementRequestModel,
	type ElementResponseModel,
	type UpdateElementRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { client } from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiDetailDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

type BatchResponseModelElementResponseModel = {
	total: number;
	items: Array<ElementResponseModel>;
};

type GetElementBatchResponses = {
	200: BatchResponseModelElementResponseModel;
};

export class UmbManagementApiElementDetailDataRequestManager extends UmbManagementApiDetailDataRequestManager<
	ElementResponseModel,
	CreateElementRequestModel,
	UpdateElementRequestModel
> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<ElementResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			create: (body: CreateElementRequestModel) => ElementService.postElement({ body }),
			read: (id: string) => ElementService.getElementById({ path: { id } }),
			update: (id: string, body: UpdateElementRequestModel) => ElementService.putElementById({ path: { id }, body }),
			delete: (id: string) => ElementService.deleteRecycleBinElementById({ path: { id } }),
			// TODO (V19): swap to ElementService.getElementBatch once the C# /element/batch endpoint ships and the SDK regenerates.
			readMany: (ids: Array<string>) =>
				client.get<GetElementBatchResponses, unknown, true>({
					security: [{ scheme: 'bearer', type: 'http' }],
					url: '/umbraco/management/api/v1/element/batch',
					query: { id: ids },
				}),
			dataCache: elementDetailCache,
			inflightRequestCache: UmbManagementApiElementDetailDataRequestManager.#inflightRequestCache,
		});
	}
}
