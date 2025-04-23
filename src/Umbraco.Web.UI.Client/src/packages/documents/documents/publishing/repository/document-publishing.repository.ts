import type { UmbDocumentDetailModel, UmbDocumentVariantPublishModel } from '../../types.js';
import { UmbDocumentPublishingServerDataSource } from './document-publishing.server.data-source.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentPublishingRepository extends UmbRepositoryBase {
	#publishingDataSource = new UmbDocumentPublishingServerDataSource(this);

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
