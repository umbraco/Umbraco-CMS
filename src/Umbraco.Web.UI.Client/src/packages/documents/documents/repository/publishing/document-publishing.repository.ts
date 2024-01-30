import { UmbDocumentPublishingServerDataSource } from './document-publishing.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UMB_NOTIFICATION_CONTEXT, type UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentPublishingRepository extends UmbRepositoryBase {
	#init!: Promise<unknown>;
	#publishingDataSource: UmbDocumentPublishingServerDataSource;
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
	 * @return {*}
	 * @memberof UmbDocumentPublishingRepository
	 */
	async publish(unique: string, variantIds: Array<UmbVariantId>) {
		if (!unique) throw new Error('id is missing');
		if (!variantIds) throw new Error('variant IDs are missing');
		await this.#init;

		const { error } = await this.#publishingDataSource.publish(unique, variantIds);

		if (!error) {
			const notification = { data: { message: `Document published` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Unpublish one or more variants of a Document
	 * @param {string} id
	 * @param {Array<UmbVariantId>} variantIds
	 * @return {*}
	 * @memberof UmbDocumentPublishingRepository
	 */
	async unpublish(id: string, variantIds: Array<UmbVariantId>) {
		if (!id) throw new Error('id is missing');
		if (!variantIds) throw new Error('variant IDs are missing');
		await this.#init;

		const { error } = await this.#publishingDataSource.unpublish(id, variantIds);

		if (!error) {
			const notification = { data: { message: `Document unpublished` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
