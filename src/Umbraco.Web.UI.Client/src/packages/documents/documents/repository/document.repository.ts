import { UmbDocumentServerDataSource } from './sources/document.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentRepository extends UmbBaseController implements UmbApi {
	#init!: Promise<unknown>;

	#detailDataSource: UmbDocumentServerDataSource;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		// TODO: figure out how spin up get the correct data source
		this.#detailDataSource = new UmbDocumentServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	// TODO: Move

	// Structure permissions;
	async requestAllowedDocumentTypesOf(id: string | null) {
		if (id === undefined) throw new Error('Id is missing');
		await this.#init;
		return this.#detailDataSource.getAllowedDocumentTypesOf(id);
	}

	// General:

	async trash(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { error } = await this.#detailDataSource.trash(id);

		if (!error) {
			const notification = { data: { message: `Document moved to recycle bin` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async publish(id: string, variantIds: Array<UmbVariantId>) {
		if (!id) throw new Error('id is missing');
		if (!variantIds) throw new Error('variant IDs are missing');
		await this.#init;

		const { error } = await this.#detailDataSource.publish(id, variantIds);

		if (!error) {
			// TODO: Update other stores based on above effect.

			const notification = { data: { message: `Document published` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async unpublish(id: string, variantIds: Array<UmbVariantId>) {
		if (!id) throw new Error('id is missing');
		if (!variantIds) throw new Error('variant IDs are missing');
		await this.#init;

		const { error } = await this.#detailDataSource.unpublish(id, variantIds);

		if (!error) {
			// TODO: Update other stores based on above effect.

			const notification = { data: { message: `Document unpublished` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
