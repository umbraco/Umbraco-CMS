import { UmbUserRepositoryBase } from '../../repository/user-repository-base.js';
import { UmbInviteUserServerDataSource } from './invite-user-server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { InviteUserRequestModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbInviteUserRepository extends UmbUserRepositoryBase {
	#inviteSource: UmbInviteUserServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#inviteSource = new UmbInviteUserServerDataSource(host);
	}

	/**
	 * Invites a user
	 * @param {InviteUserRequestModel} requestModel
	 * @return {*}
	 * @memberof UmbInviteUserRepository
	 */
	async invite(requestModel: InviteUserRequestModel) {
		if (!requestModel) throw new Error('data is missing');
		await this.init;

		const { data, error } = await this.#inviteSource.invite(requestModel);

		if (data) {
			this.detailStore!.append(data);

			const notification = { data: { message: `Invite sent to user` } };
			this.notificationContext?.peek('positive', notification);
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
		await this.init;

		const { error } = await this.#inviteSource.resendInvite(userId, requestModel);

		if (!error) {
			const notification = { data: { message: `Invite resent to user` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
