import { documentItemCache } from './document-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiDocumentItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<DocumentItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: documentItemCache,
			serverEventSources: ['Umbraco:CMS:Document'],
		});
	}
}
