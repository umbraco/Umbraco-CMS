import { documentTypeDetailCache } from './document-type-detail.server.cache.js';
import { UmbManagementApiDetailDataCacheInvalidationManager } from '@umbraco-cms/backoffice/management-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbManagementApiDocumentTypeDetailDataCacheInvalidationManager extends UmbManagementApiDetailDataCacheInvalidationManager<DocumentTypeResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			dataCache: documentTypeDetailCache,
			eventSources: ['Umbraco:CMS:DocumentType'],
		});
	}
}
