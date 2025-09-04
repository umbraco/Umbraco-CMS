import { mediaTypeItemCache } from './media-type-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { MediaTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiMediaTypeItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<MediaTypeItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: mediaTypeItemCache,
			eventSources: ['Umbraco:CMS:MediaType'],
		});
	}
}
