import { UmbChangeUserPasswordServerDataSource } from './change-user-password.server.data.js';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';

export class UmbChangeUserPasswordRepository {
	#host: UmbControllerHostElement;
	#init!: Promise<unknown>;

	#changePasswordSource: UmbChangeUserPasswordServerDataSource;
	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#changePasswordSource = new UmbChangeUserPasswordServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	async changePassword(userId: string, newPassword: string) {
		if (!userId) throw new Error('User id is missing');
		if (!newPassword) throw new Error('New password is missing');
		await this.#init;

		const { data, error } = await this.#changePasswordSource.changePassword(userId, newPassword);

		if (!error) {
			const notification = { data: { message: `Password changed` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}
