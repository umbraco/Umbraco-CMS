import { UmbDocumentCultureAndHostnamesServerDataSource } from './culture-and-hostnames.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { DomainsPresentationModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDocumentCultureAndHostnamesRepository extends UmbBaseController implements UmbApi {
	#init!: Promise<unknown>;

	#dataSource: UmbDocumentCultureAndHostnamesServerDataSource;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbDocumentCultureAndHostnamesServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.#notificationContext = instance as UmbNotificationContext;
			}).asPromise(),
		]);
	}

	async readCultureAndHostnames(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;

		const { data, error } = await this.#dataSource.read(id);
		if (!error) {
			return { data };
		}
		return { error };
	}

	async updateCultureAndHostnames(id: string, data: DomainsPresentationModelBaseModel) {
		if (!id) throw new Error('Id is missing');
		if (!data) throw new Error('Data is missing');
		await this.#init;

		const { error } = await this.#dataSource.update(id, data);
		if (!error) {
			const notification = { data: { message: `Cultures and hostnames saved` } };
			this.#notificationContext?.peek('positive', notification);
		}
		return { error };
	}
}
