import { templateItemCache } from './template-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { TemplateItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiTemplateItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<TemplateItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: templateItemCache,
			eventSources: ['Umbraco:CMS:Template'],
		});
	}
}
