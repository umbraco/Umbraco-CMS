import { type UmbInviteUserDataSource } from './types.js';
import { UmbInviteUserServerDataSource } from './invite-user.server.data.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { InviteUserRequestModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbInviteUserRepository {
	#host: UmbControllerHostElement;
	#init;

	#inviteSource: UmbInviteUserDataSource;
	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#inviteSource = new UmbInviteUserServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	/**
	 * Invites a user
	 * @param {InviteUserRequestModel} requestModel
	 * @return {*}
	 * @memberof UmbInviteUserRepository
	 */
	async invite(requestModel: InviteUserRequestModel) {
		if (!requestModel) throw new Error('data is missing');
		await this.#init;

		const { error } = await this.#inviteSource.invite(requestModel);

		if (!error) {
			const notification = { data: { message: `Invite sent to user` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Resend an invite to a user
	 * @param {string} userId
	 * @param {InviteUserRequestModel} requestModel
	 * @return {*}
	 * @memberof UmbInviteUserRepository
	 */
	async resendInvite(userId: string, requestModel: any) {
		if (!userId) throw new Error('User id is missing');
		if (!requestModel) throw new Error('data is missing');
		await this.#init;

		const { error } = await this.#inviteSource.resendInvite(userId, requestModel);

		if (!error) {
			const notification = { data: { message: `Invite resent to user` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
