import { UMB_USER_STORE_CONTEXT_TOKEN, UmbUserStore } from '../user.store.js';
import { UMB_USER_ITEM_STORE_CONTEXT_TOKEN, UmbUserItemStore } from '../user-item.store.js';
import { UmbDisableUserServerDataSource } from './disable-user.server.data.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDisableUserRepository {
	#host: UmbControllerHostElement;
	#init;

	#disableSource: UmbDisableUserServerDataSource;
	#notificationContext?: UmbNotificationContext;
	#detailStore?: UmbUserStore;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#disableSource = new UmbDisableUserServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_USER_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	async disable(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');
		await this.#init;

		const { data, error } = await this.#disableSource.disable(ids);

		if (!error) {
			ids.forEach((id) => {
				this.#detailStore?.updateItem(id, { state: UserStateModel.DISABLED });
			});

			const notification = { data: { message: `User disabled` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}
