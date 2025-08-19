/* eslint-disable local-rules/no-direct-api-import */
import { documentTypeItemCache } from './document-type-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DocumentTypeService, type DocumentTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiDocumentTypeItemDataRequestManager extends UmbManagementApiItemDataRequestManager<DocumentTypeItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => DocumentTypeService.getItemDocumentType({ query: { id: ids } }),
			dataCache: documentTypeItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
