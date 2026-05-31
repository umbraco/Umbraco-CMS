import { mediaTypeDetailCache } from './media-type-detail.server.cache.js';
import { UmbManagementApiDetailDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { MediaTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiMediaTypeDetailDataCacheInvalidationManager extends UmbManagementApiDetailDataCacheInvalidationManager<MediaTypeResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: mediaTypeDetailCache,
			eventSources: ['Umbraco:CMS:MediaType'],
		});
	}
}
