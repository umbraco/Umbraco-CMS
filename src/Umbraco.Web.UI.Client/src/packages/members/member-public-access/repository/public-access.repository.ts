import { UmbDocumentPublicAccessServerDataSource } from './public-access.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { PublicAccessRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbDocumentPublicAccessRepository extends UmbControllerBase implements UmbApi {
	#dataSource: UmbDocumentPublicAccessServerDataSource;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbDocumentPublicAccessServerDataSource(this);

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
			this.#notificationContext = instance as UmbNotificationContext;
		});
	}

	async create(unique: string, data: PublicAccessRequestModel) {
		if (!unique) throw new Error('unique is missing');
		if (!data) throw new Error('Data is missing');

		const { error } = await this.#dataSource.create(unique, data);
		if (!error) {
			const notification = { data: { message: `Public access setting created` } };
			this.#notificationContext?.peek('positive', notification);
		}
		return { error };
	}

	async read(unique: string) {
		if (!unique) throw new Error('unique is missing');

		const { data, error } = await this.#dataSource.read(unique);
		return { data, error };
	}

	async update(unique: string, data: PublicAccessRequestModel) {
		if (!unique) throw new Error('unique is missing');
		if (!data) throw new Error('Data is missing');

		const { error } = await this.#dataSource.update(unique, data);
		if (!error) {
			const notification = { data: { message: `Public access setting updated` } };
			this.#notificationContext?.peek('positive', notification);
		}
		return { error };
	}

	async delete(unique: string) {
		if (!unique) throw new Error('unique is missing');

		const { error } = await this.#dataSource.delete(unique);
		if (!error) {
			const notification = { data: { message: `Public access setting deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}
		return { error };
	}
}

export { UmbDocumentPublicAccessRepository as api };
