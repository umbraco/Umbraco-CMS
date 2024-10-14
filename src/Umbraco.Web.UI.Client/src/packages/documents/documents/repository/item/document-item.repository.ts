import { UmbDocumentItemServerDataSource } from './document-item.server.data-source.js';
import { UMB_DOCUMENT_ITEM_STORE_CONTEXT } from './document-item.store.context-token.js';
import type { UmbDocumentItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentItemRepository extends UmbItemRepositoryBase<UmbDocumentItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentItemServerDataSource, UMB_DOCUMENT_ITEM_STORE_CONTEXT);
	}
}

export { UmbDocumentItemRepository as api };
