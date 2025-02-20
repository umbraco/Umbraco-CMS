import { UmbDocumentCultureAndHostnamesServerDataSource } from './culture-and-hostnames.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UpdateDomainsRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbDocumentCultureAndHostnamesRepository extends UmbControllerBase implements UmbApi {
	#dataSource = new UmbDocumentCultureAndHostnamesServerDataSource(this);

	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
			this.#notificationContext = instance;
		});
	}

	async readCultureAndHostnames(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await this.#dataSource.read(unique);
		if (!error) {
			return { data };
		}
		return { error };
	}

	async updateCultureAndHostnames(unique: string, data: UpdateDomainsRequestModel) {
		if (!unique) throw new Error('Unique is missing');
		if (!data) throw new Error('Data is missing');

		const { error } = await this.#dataSource.update(unique, data);
		if (!error) {
			const notification = { data: { message: `Cultures and hostnames saved` } };
			this.#notificationContext?.peek('positive', notification);
		}
		return { error };
	}
}

export { UmbDocumentCultureAndHostnamesRepository as api };
