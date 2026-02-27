/* eslint-disable local-rules/no-direct-api-import */
import { mediaTypeDetailCache } from './media-type-detail.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	MediaTypeService,
	type CreateMediaTypeRequestModel,
	type MediaTypeResponseModel,
	type UpdateMediaTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import {
	UmbManagementApiDetailDataRequestManager,
	UmbManagementApiInFlightRequestCache,
} from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiMediaTypeDetailDataRequestManager extends UmbManagementApiDetailDataRequestManager<
	MediaTypeResponseModel,
	UpdateMediaTypeRequestModel,
	CreateMediaTypeRequestModel
> {
	static #inflightRequestCache = new UmbManagementApiInFlightRequestCache<MediaTypeResponseModel>();

	constructor(host: UmbControllerHost) {
		super(host, {
			create: (body: CreateMediaTypeRequestModel) => MediaTypeService.postMediaType({ body }),
			read: (id: string) => MediaTypeService.getMediaTypeById({ path: { id } }),
			update: (id: string, body: UpdateMediaTypeRequestModel) =>
				MediaTypeService.putMediaTypeById({ path: { id }, body }),
			delete: (id: string) => MediaTypeService.deleteMediaTypeById({ path: { id } }),
			readMany: (ids: Array<string>) => MediaTypeService.getMediaTypeBatch({ query: { id: ids } }),
			dataCache: mediaTypeDetailCache,
			inflightRequestCache: UmbManagementApiMediaTypeDetailDataRequestManager.#inflightRequestCache,
		});
	}
}
