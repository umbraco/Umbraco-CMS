import { UmbDocumentCultureAndHostnamesServerDataSource } from './culture-and-hostnames.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { DomainsPresentationModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDocumentCultureAndHostnamesRepository extends UmbBaseController implements UmbApi {
	#init!: Promise<unknown>;

	#dataSource = new UmbDocumentCultureAndHostnamesServerDataSource(this);

	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = Promise.all([
			this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	async readCultureAndHostnames(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		await this.#init;

		const { data, error } = await this.#dataSource.read(unique);
		if (!error) {
			return { data };
		}
		return { error };
	}

	async updateCultureAndHostnames(unique: string, data: DomainsPresentationModelBaseModel) {
		if (!unique) throw new Error('Unique is missing');
		if (!data) throw new Error('Data is missing');
		await this.#init;

		const { error } = await this.#dataSource.update(unique, data);
		if (!error) {
			const notification = { data: { message: `Cultures and hostnames saved` } };
			this.#notificationContext?.peek('positive', notification);
		}
		return { error };
	}
}
