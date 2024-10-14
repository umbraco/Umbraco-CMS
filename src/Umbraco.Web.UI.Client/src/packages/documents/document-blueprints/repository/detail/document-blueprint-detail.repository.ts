import type { UmbDocumentBlueprintDetailModel } from '../../types.js';
import { UmbDocumentBlueprintServerDataSource } from './document-blueprint-detail.server.data-source.js';
import { UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT } from './document-blueprint-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentBlueprintDetailRepository extends UmbDetailRepositoryBase<UmbDocumentBlueprintDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentBlueprintServerDataSource, UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT);
	}
}

export { UmbDocumentBlueprintDetailRepository as api };
