import { staticFileItemCache } from './static-file-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { StaticFileItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiStaticFileItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<StaticFileItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: staticFileItemCache,
			eventSources: ['Umbraco:CMS:StaticFile'],
		});
	}
}
