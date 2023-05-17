import { UmbCultureServerDataSource } from './sources/culture.server.data';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from 'src/packages/core/notification';

export class UmbCultureRepository {
	#init!: Promise<unknown>;
	#host: UmbControllerHostElement;

	#dataSource: UmbCultureServerDataSource;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#dataSource = new UmbCultureServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}),
		]);
	}

	requestCultures({ skip, take } = { skip: 0, take: 1000 }) {
		return this.#dataSource.getCollection({ skip, take });
	}
}
