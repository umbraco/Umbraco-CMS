import type { UmbDocumentDetailModel, UmbDocumentVariantPublishModel } from '../../types.js';
import { UmbDocumentPublishingServerDataSource } from './document-publishing.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import { UMB_NOTIFICATION_CONTEXT, type UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentPublishingRepository extends UmbRepositoryBase {
	#init!: Promise<unknown>;
	#publishingDataSource: UmbDocumentPublishingServerDataSource;

	/**
	 * @deprecated The calling workspace context should be used instead to show notifications
	 */
	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#publishingDataSource = new UmbDocumentPublishingServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
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
		await this.#init;

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
		await this.#init;

		const { error } = await this.#publishingDataSource.unpublish(id, variantIds);

		if (!error) {
			const notification = { data: { message: `Document unpublished` } };
			// TODO: Move this to the calling workspace context [JOV]
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Publish variants of a document including its descendants
	 * @param id
	 * @param variantIds
	 * @param includeUnpublishedDescendants
	 * @param forceRepublish
	 * @memberof UmbDocumentPublishingRepository
	 */
	async publishWithDescendants(
		id: string,
		variantIds: Array<UmbVariantId>,
		includeUnpublishedDescendants: boolean,
		forceRepublish: boolean,
	) {
		if (!id) throw new Error('id is missing');
		if (!variantIds) throw new Error('variant IDs are missing');
		await this.#init;

		const { error } = await this.#publishingDataSource.publishWithDescendants(
			id,
			variantIds,
			includeUnpublishedDescendants,
			forceRepublish,
		);

		if (!error) {
			const notification = { data: { message: `Document published with descendants` } };
			// TODO: Move this to the calling workspace context [JOV]
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
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
