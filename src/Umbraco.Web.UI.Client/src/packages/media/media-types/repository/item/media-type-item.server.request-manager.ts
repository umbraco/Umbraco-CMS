/* eslint-disable local-rules/no-direct-api-import */
import { mediaTypeItemCache } from './media-type-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MediaTypeService, type MediaTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiMediaTypeItemDataRequestManager extends UmbManagementApiItemDataRequestManager<MediaTypeItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => MediaTypeService.getItemMediaType({ query: { id: ids } }),
			dataCache: mediaTypeItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
