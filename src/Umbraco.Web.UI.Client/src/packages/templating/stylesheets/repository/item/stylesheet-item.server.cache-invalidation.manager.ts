import { stylesheetItemCache } from './stylesheet-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { StylesheetItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiStylesheetItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<StylesheetItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: stylesheetItemCache,
			eventSources: ['Umbraco:CMS:Stylesheet'],
		});
	}
}
