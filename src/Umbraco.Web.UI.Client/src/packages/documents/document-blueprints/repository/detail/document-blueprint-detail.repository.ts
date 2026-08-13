import type { UmbDocumentBlueprintDetailModel } from '../../types.js';
import { UmbDocumentBlueprintServerDataSource } from './document-blueprint-detail.server.data-source.js';
import { UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT } from './document-blueprint-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentBlueprintDetailRepository extends UmbDetailRepositoryBase<
	UmbDocumentBlueprintDetailModel,
	UmbDocumentBlueprintServerDataSource
> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentBlueprintServerDataSource, UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT);
	}

	/**
	 * Gets an existing document blueprint by its unique identifier for scaffolding purposes, i.e. to create a new document based on an existing blueprint.
	 * @param {string} unique - The unique identifier of the document blueprint.
	 * @returns {Promise<UmbRepositoryResponse<UmbDocumentBlueprintDetailModel>>} - The document blueprint data.
	 * @memberof UmbDocumentBlueprintDetailRepository
	 */
	scaffoldByUnique(unique: string): Promise<UmbRepositoryResponse<UmbDocumentBlueprintDetailModel>> {
		return this.detailDataSource.scaffoldByUnique(unique);
	}
}

export { UmbDocumentBlueprintDetailRepository as api };
