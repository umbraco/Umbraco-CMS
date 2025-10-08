import { dictionaryItemCache } from './dictionary-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DictionaryItemItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiDictionaryItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<DictionaryItemItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: dictionaryItemCache,
			eventSources: ['Umbraco:CMS:DictionaryItem'],
		});
	}
}
