import { UmbUserServerDataSource } from '../../repository/detail/user-detail.server.data-source.js';
import type { UmbInviteUserDataSource } from './types.js';
import type {
	InviteUserRequestModel,
	ResendInviteUserRequestModel} from '@umbraco-cms/backoffice/backend-api';
import {
	UserResource,
} from '@umbraco-cms/backoffice/backend-api';
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
	 * @param {string} userUnique
	 * @param {InviteUserRequestModel} requestModel
	 * @returns
	 * @memberof UmbInviteUserServerDataSource
	 */
	async resendInvite(requestModel: ResendInviteUserRequestModel) {
		if (!requestModel.userId) throw new Error('User id is missing');
		if (!requestModel) throw new Error('Data is missing');

		return tryExecuteAndNotify(
			this.#host,
			UserResource.postUserInviteResend({
				requestBody: requestModel,
			}),
		);
	}
}
