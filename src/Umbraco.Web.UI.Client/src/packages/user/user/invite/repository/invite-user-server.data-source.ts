import { UmbUserServerDataSource } from '../../repository/detail/user-detail.server.data-source.js';
import { type UmbInviteUserDataSource } from './types.js';
import { InviteUserRequestModel, UserResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for inviting users
 * @export
 * @class UmbInviteUserServerDataSource
 */
export class UmbInviteUserServerDataSource implements UmbInviteUserDataSource {
	#host: UmbControllerHost;
	#detailSource: UmbUserServerDataSource;

	/**
	 * Creates an instance of UmbInviteUserServerDataSource.
	 * @memberof UmbInviteUserServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.#detailSource = new UmbUserServerDataSource(host);
	}

	/**
	 * Invites a user
	 * @param {InviteUserRequestModel} requestModel
	 * @returns
	 * @memberof UmbInviteUserServerDataSource
	 */
	async invite(requestModel: InviteUserRequestModel) {
		if (!requestModel) throw new Error('Data is missing');

		const { data: newUserId, error } = await tryExecuteAndNotify(
			this.#host,
			UserResource.postUserInvite({
				requestBody: requestModel,
			}),
		);

		if (newUserId) {
			return this.#detailSource.read(newUserId);
		}

		return { error };
	}

	/**
	 * Resend an invite to a user
	 * @param {string} userId
	 * @param {InviteUserRequestModel} requestModel
	 * @returns
	 * @memberof UmbInviteUserServerDataSource
	 */
	async resendInvite(userId: string, requestModel: InviteUserRequestModel) {
		if (!userId) throw new Error('User id is missing');
		if (!requestModel) throw new Error('Data is missing');

		alert('End point is missing');

		const body = JSON.stringify({
			userId,
			requestModel,
		});

		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/user/invite/resend', {
				method: 'POST',
				body: body,
				headers: {
					'Content-Type': 'application/json',
				},
			}).then((res) => res.json()),
		);
	}
}
