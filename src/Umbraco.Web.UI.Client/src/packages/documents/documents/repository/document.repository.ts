import { UmbDocumentServerDataSource } from './sources/document.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

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
}
