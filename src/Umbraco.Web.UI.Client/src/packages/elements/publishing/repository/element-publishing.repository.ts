import type { UmbElementVariantPublishModel } from '../types.js';
import { UmbElementPublishingServerDataSource } from './element-publishing.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbElementPublishingRepository extends UmbRepositoryBase {
	#publishingDataSource = new UmbElementPublishingServerDataSource(this);

	/**
	 * Publish one or more variants of an Element
	 * @param {string} unique
	 * @param {Array<UmbElementVariantPublishModel>} variants
	 * @returns {*}
	 * @memberof UmbElementPublishingRepository
	 */
	async publish(unique: string, variants: Array<UmbElementVariantPublishModel>) {
		if (!unique) throw new Error('id is missing');
		if (!variants.length) throw new Error('variant IDs are missing');

		return this.#publishingDataSource.publish(unique, variants);
	}

	/**
	 * Unpublish one or more variants of an Element
	 * @param {string} id
	 * @param {Array<UmbVariantId>} variantIds
	 * @returns {*}
	 * @memberof UmbElementPublishingRepository
	 */
	async unpublish(id: string, variantIds: Array<UmbVariantId>) {
		if (!id) throw new Error('id is missing');
		if (!variantIds) throw new Error('variant IDs are missing');

		return this.#publishingDataSource.unpublish(id, variantIds);
	}
}

export { UmbElementPublishingRepository as api };
