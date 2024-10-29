import type { UmbDocumentUrlsModel } from './types.js';
import { UMB_DOCUMENT_URL_STORE_CONTEXT } from './document-url.store.context-token.js';
import { UmbDocumentUrlServerDataSource } from './document-url.server.data-source.js';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentUrlRepository extends UmbItemRepositoryBase<UmbDocumentUrlsModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentUrlServerDataSource, UMB_DOCUMENT_URL_STORE_CONTEXT);
	}
}

export { UmbDocumentUrlRepository as api };
