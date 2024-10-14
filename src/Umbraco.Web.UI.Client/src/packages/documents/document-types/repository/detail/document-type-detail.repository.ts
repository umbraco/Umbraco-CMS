import type { UmbDocumentTypeDetailModel } from '../../types.js';
import { UmbDocumentTypeDetailServerDataSource } from './document-type-detail.server.data-source.js';
import { UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT } from './document-type-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
export class UmbDocumentTypeDetailRepository extends UmbDetailRepositoryBase<UmbDocumentTypeDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTypeDetailServerDataSource, UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT);
	}
}

export { UmbDocumentTypeDetailRepository as api };
