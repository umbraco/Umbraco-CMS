import type { UmbDocumentDetailModel } from '../../types.js';
import { UmbDocumentServerDataSource } from './document-detail.server.data-source.js';
import { UMB_DOCUMENT_DETAIL_STORE_CONTEXT } from './document-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
export class UmbDocumentDetailRepository extends UmbDetailRepositoryBase<
	UmbDocumentDetailModel,
	UmbDocumentServerDataSource
> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentServerDataSource, UMB_DOCUMENT_DETAIL_STORE_CONTEXT);
	}

	/**
	 * Creates and publishes a new Document in a single operation
	 * @param {UmbDocumentDetailModel} model - The Document to create
	 * @param {Array<UmbVariantId>} variantIds - The variants to publish after creating
	 * @param {string | null} parentUnique - The unique of the parent to create under
	 * @returns {*}
	 * @memberof UmbDocumentDetailRepository
	 */
	async createAndPublish(model: UmbDocumentDetailModel, variantIds: Array<UmbVariantId>, parentUnique: string | null) {
		return this.detailDataSource.createAndPublish(model, variantIds, parentUnique);
	}

	/**
	 * Updates and publishes an existing Document in a single operation
	 * @param {UmbDocumentDetailModel} model - The Document to update
	 * @param {Array<UmbVariantId>} variantIds - The variants to publish after updating
	 * @returns {*}
	 * @memberof UmbDocumentDetailRepository
	 */
	async updateAndPublish(model: UmbDocumentDetailModel, variantIds: Array<UmbVariantId>) {
		return this.detailDataSource.updateAndPublish(model, variantIds);
	}
}

export { UmbDocumentDetailRepository as api };
