import type { UmbDocumentDetailModel } from '../../types.js';
import { UmbDocumentServerDataSource } from './document-detail.server.data-source.js';
import { UMB_DOCUMENT_DETAIL_STORE_CONTEXT } from './document-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
export class UmbDocumentDetailRepository extends UmbDetailRepositoryBase<UmbDocumentDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentServerDataSource, UMB_DOCUMENT_DETAIL_STORE_CONTEXT);
	}
}

export { UmbDocumentDetailRepository as api };
