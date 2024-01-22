import { UmbDocumentItemServerDataSource } from './document-item.server.data.js';
import { UMB_DOCUMENT_ITEM_STORE_CONTEXT } from './document-item.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { DocumentItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDocumentItemRepository extends UmbItemRepositoryBase<DocumentItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentItemServerDataSource, UMB_DOCUMENT_ITEM_STORE_CONTEXT);
	}
}
