import { UmbDocumentTypeDetailModel } from '../../types.js';
import { UmbDocumentTypeServerDataSource } from './document-type-detail.server.data-source.js';
import { UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT } from './document-type-detail.store.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
export class UmbDocumentTypeDetailRepository extends UmbDetailRepositoryBase<UmbDocumentTypeDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentTypeServerDataSource, UMB_DOCUMENT_TYPE_DETAIL_STORE_CONTEXT);
	}
}
