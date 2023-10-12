import { UMB_USER_STORE_CONTEXT_TOKEN, type UmbUserStore } from '../user.store.js';
import { UmbEnableUserServerDataSource } from './enable-user.server.data.js';
import { type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

export class UmbEnableUserRepository {
	#host: UmbControllerHostElement;
	#init;

	#enableSource: UmbEnableUserServerDataSource;
	#detailStore?: UmbUserStore;
	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#enableSource = new UmbEnableUserServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_USER_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	async enable(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');
		await this.#init;

		const { data, error } = await this.#enableSource.enable(ids);

		if (!error) {
			ids.forEach((id) => {
				this.#detailStore?.updateItem(id, { state: UserStateModel.ACTIVE });
			});

			const notification = { data: { message: `User disabled` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}
