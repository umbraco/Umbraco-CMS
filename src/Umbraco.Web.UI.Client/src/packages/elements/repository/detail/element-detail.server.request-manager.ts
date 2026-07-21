import { elementDetailCache } from './element-detail.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	ElementService,
	type CreateElementRequestModel,
	type ElementResponseModel,
	type UpdateElementRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiDetailDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

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
			readMany: (ids: Array<string>) => ElementService.getElementBatch({ query: { id: ids } }),
			dataCache: elementDetailCache,
			inflightRequestCache: UmbManagementApiElementDetailDataRequestManager.#inflightRequestCache,
		});
	}
}
