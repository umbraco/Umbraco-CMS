import { UmbDocumentCultureAndHostnamesServerDataSource } from './culture-and-hostnames.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentCultureAndHostnamesRepository extends UmbBaseController implements UmbApi {
	#init!: Promise<unknown>;

	#detailDataSource: UmbDocumentCultureAndHostnamesServerDataSource;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#detailDataSource = new UmbDocumentCultureAndHostnamesServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	async readCultureAndHostnames(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { data, error } = await this.#detailDataSource.read(id);
		if (!error) {
			return { data };
		}
		return { error };
	}

	async updateCultureAndHostnames(id: string, data: any) {
		if (!id) throw new Error('Id is missing');
		if (!data) throw new Error('Data is missing');
		await this.#init;

		const { error } = await this.#detailDataSource.update(id, data);
		if (!error) {
			const notification = { data: { message: `Cultures and hostnames saved` } };
			this.#notificationContext?.peek('positive', notification);
		}
		return { error };
	}
}
