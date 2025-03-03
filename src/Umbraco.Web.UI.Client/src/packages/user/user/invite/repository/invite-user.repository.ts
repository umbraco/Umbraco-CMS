import { UmbUserRepositoryBase } from '../../repository/user-repository-base.js';
import { UmbInviteUserServerDataSource } from './invite-user-server.data-source.js';
import type { UmbInviteUserRequestModel, UmbResendUserInviteRequestModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbInviteUserRepository extends UmbUserRepositoryBase {
	#inviteSource: UmbInviteUserServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#inviteSource = new UmbInviteUserServerDataSource(host);
	}

	/**
	 * Invites a user
	 * @param {UmbInviteUserRequestModel} request
	 * @returns {*}
	 * @memberof UmbInviteUserRepository
	 */
	async invite(request: UmbInviteUserRequestModel) {
		if (!request) throw new Error('Request data is missing');
		await this.init;

		const { data, error } = await this.#inviteSource.invite(request);

		if (data) {
			this.detailStore!.append(data);

			const notification = { data: { message: `Invite sent to user` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	/**
	 * Resend an invite to a user
	 * @param {string} userUnique
	 * @param {InviteUserRequestModel} request
	 * @returns {*}
	 * @memberof UmbInviteUserRepository
	 */
	async resendInvite(request: UmbResendUserInviteRequestModel) {
		if (!request.user.unique) throw new Error('User unique is missing');
		if (!request) throw new Error('data is missing');
		await this.init;

		const { error } = await this.#inviteSource.resendInvite(request);

		if (!error) {
			const notification = { data: { message: `Invite resent to user` } };
			this.notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}

export default UmbInviteUserRepository;
