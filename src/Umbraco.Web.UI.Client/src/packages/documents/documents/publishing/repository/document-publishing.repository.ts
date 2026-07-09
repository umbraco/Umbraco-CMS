import type { UmbDocumentDetailModel, UmbDocumentVariantPublishModel } from '../../types.js';
import { UmbDocumentPublishingServerDataSource } from './document-publishing.server.data-source.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentPublishingRepository extends UmbRepositoryBase {
	#publishingDataSource = new UmbDocumentPublishingServerDataSource(this);

	/**
	 * Creates and publishes a new Document in a single operation
	 * @param {UmbDocumentDetailModel} model - The Document to create
	 * @param {Array<UmbVariantId>} variantIds - The variants to publish after creating
	 * @param {string | null} parentUnique - The unique of the parent to create under
	 * @returns {*}
	 * @memberof UmbDocumentPublishingRepository
	 */
	async createAndPublish(
		model: UmbDocumentDetailModel,
		variantIds: Array<UmbVariantId>,
		parentUnique: string | null = null,
	) {
		if (!model) throw new Error('Document is missing');
		if (!model.unique) throw new Error('Document unique is missing');

		return this.#publishingDataSource.createAndPublish(model, variantIds, parentUnique);
	}

	/**
	 * Updates and publishes an existing Document in a single operation
	 * @param {UmbDocumentDetailModel} model - The Document to update
	 * @param {Array<UmbVariantId>} variantIds - The variants to publish after updating
	 * @returns {*}
	 * @memberof UmbDocumentPublishingRepository
	 */
	async updateAndPublish(model: UmbDocumentDetailModel, variantIds: Array<UmbVariantId>) {
		if (!model.unique) throw new Error('Unique is missing');

		return this.#publishingDataSource.updateAndPublish(model, variantIds);
	}

	/**
	 * Publish one or more variants of a Document
	 * @param {string} id
	 * @param {Array<UmbVariantId>} variantIds
	 * @param unique
	 * @param variants
	 * @returns {*}
	 * @memberof UmbDocumentPublishingRepository
	 */
	async publish(unique: string, variants: Array<UmbDocumentVariantPublishModel>) {
		if (!unique) throw new Error('id is missing');
		if (!variants.length) throw new Error('variant IDs are missing');

		return this.#publishingDataSource.publish(unique, variants);
	}

	/**
	 * Unpublish one or more variants of a Document
	 * @param {string} id
	 * @param {Array<UmbVariantId>} variantIds
	 * @returns {*}
	 * @memberof UmbDocumentPublishingRepository
	 */
	async unpublish(id: string, variantIds: Array<UmbVariantId>) {
		if (!id) throw new Error('id is missing');
		if (!variantIds) throw new Error('variant IDs are missing');

		return this.#publishingDataSource.unpublish(id, variantIds);
	}

	/**
	 * Publish variants of a document including its descendants
	 * @param id
	 * @param variantIds
	 * @param includeUnpublishedDescendants
	 * @memberof UmbDocumentPublishingRepository
	 */
	async publishWithDescendants(id: string, variantIds: Array<UmbVariantId>, includeUnpublishedDescendants: boolean) {
		if (!id) throw new Error('id is missing');
		if (!variantIds) throw new Error('variant IDs are missing');

		return this.#publishingDataSource.publishWithDescendants(id, variantIds, includeUnpublishedDescendants);
	}

	/**
	 * Get the published data of a document
	 * @param {string} unique Document unique
	 * @returns { Promise<UmbRepositoryResponse<UmbDocumentDetailModel>>} Published document
	 * @memberof UmbDocumentPublishingRepository
	 */
	async published(unique: string): Promise<UmbRepositoryResponse<UmbDocumentDetailModel>> {
		return this.#publishingDataSource.published(unique);
	}
}

export { UmbDocumentPublishingRepository as api };
