import { UMB_USER_STORE_CONTEXT_TOKEN, UmbUserStore } from '../user.store.js';
import { type UmbInviteUserDataSource } from './types.js';
import { UmbInviteUserServerDataSource } from './invite-user.server.data.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { InviteUserRequestModel, UserStateModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbInviteUserRepository {
	#host: UmbControllerHostElement;
	#init;

	#inviteSource: UmbInviteUserDataSource;
	#notificationContext?: UmbNotificationContext;
	#detailStore?: UmbUserStore;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#inviteSource = new UmbInviteUserServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_USER_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	async invite(data: InviteUserRequestModel) {
		if (!data) throw new Error('data is missing');
		await this.#init;

		const { error } = await this.#inviteSource.invite(ids);

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
