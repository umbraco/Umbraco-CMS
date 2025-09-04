/* eslint-disable local-rules/no-direct-api-import */
import { documentItemCache } from './document-item.server.cache.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DocumentService, type DocumentItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbManagementApiItemDataRequestManager } from '@umbraco-cms/backoffice/management-api';

export class UmbManagementApiDocumentItemDataRequestManager extends UmbManagementApiItemDataRequestManager<DocumentItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems: (ids: Array<string>) => DocumentService.getItemDocument({ query: { id: ids } }),
			dataCache: documentItemCache,
			getUniqueMethod: (item) => item.id,
		});
	}
}
