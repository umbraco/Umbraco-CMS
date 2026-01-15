import { languageItemCache } from './language-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { LanguageItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiLanguageItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<LanguageItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: languageItemCache,
			eventSources: ['Umbraco:CMS:Language'],
		});
	}
}
