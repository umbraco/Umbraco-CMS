import { documentTypeItemCache } from './document-type-item.server.cache.js';
import { UmbManagementApiItemDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiDocumentTypeItemDataCacheInvalidationManager extends UmbManagementApiItemDataCacheInvalidationManager<DocumentTypeItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: documentTypeItemCache,
			eventSources: ['Umbraco:CMS:DocumentType'],
		});
	}
}
