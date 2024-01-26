import { UmbDocumentPublicAccessServerDataSource } from './public-access.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { PublicAccessRequestModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDocumentPublicAccessRepository extends UmbBaseController implements UmbApi {
	#init!: Promise<unknown>;

	#dataSource: UmbDocumentPublicAccessServerDataSource;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbDocumentPublicAccessServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.#notificationContext = instance as UmbNotificationContext;
			}).asPromise(),
		]);
	}

	async readPublicAccess(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { data, error } = await this.#dataSource.read(id);
		if (!error) {
			return { data };
		}
		return { error };
	}

	async updatePublicAccess(id: string, data: PublicAccessRequestModel) {
		if (!id) throw new Error('Id is missing');
		if (!data) throw new Error('Data is missing');
		await this.#init;

		const { error } = await this.#dataSource.update(id, data);
		if (!error) {
			const notification = { data: { message: `Public acccess saved` } };
			this.#notificationContext?.peek('positive', notification);
		}
		return { error };
	}
}
