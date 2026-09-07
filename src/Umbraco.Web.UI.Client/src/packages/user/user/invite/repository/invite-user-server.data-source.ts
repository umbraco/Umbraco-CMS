import { UmbUserServerDataSource } from '../../repository/detail/user-detail.server.data-source.js';
import type { UmbUserDetailModel } from '../../types.js';
import type { UmbInviteUserDataSource, UmbInviteUserRequestModel, UmbResendUserInviteRequestModel } from './types.js';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse, UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for inviting users
 * @class UmbInviteUserServerDataSource
 */
export class UmbInviteUserServerDataSource implements UmbInviteUserDataSource {
	#host: UmbControllerHost;
	#detailSource: UmbUserServerDataSource;

	/**
	 * Creates an instance of UmbInviteUserServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to.
	 * @memberof UmbInviteUserServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.#detailSource = new UmbUserServerDataSource(host);
	}

	/**
	 * Invites a user
	 * @param {UmbInviteUserRequestModel} request - The invite request data.
	 * @returns {Promise<UmbDataSourceResponse<UmbUserDetailModel>>} The invited user.
	 * @memberof UmbInviteUserServerDataSource
	 */
	async invite(request: UmbInviteUserRequestModel): Promise<UmbDataSourceResponse<UmbUserDetailModel>> {
		if (!request) throw new Error('Request Data is missing');

		const body = {
			email: request.email,
			userName: request.userName,
			name: request.name,
			userGroupIds: request.userGroupUniques.map((reference) => {
				return { id: reference.unique };
			}),
			message: request.message,
		};

		const { data, error } = await tryExecute(
			this.#host,
			UserService.postUserInvite({
				body,
			}),
		);

		if (data) {
			return this.#detailSource.read(data as never);
		}

		return { error };
	}

	/**
	 * Resend an invite to a user
	 * @param {UmbResendUserInviteRequestModel} request - The resend invite request data.
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of the resend invite operation.
	 * @memberof UmbInviteUserServerDataSource
	 */
	async resendInvite(request: UmbResendUserInviteRequestModel): Promise<UmbDataSourceErrorResponse> {
		if (!request.user.unique) throw new Error('User unique is missing');
		if (!request) throw new Error('Request data is missing');

		const body = {
			user: {
				id: request.user.unique,
			},
			message: request.message,
		};

		return tryExecute(
			this.#host,
			UserService.postUserInviteResend({
				body,
			}),
		);
	}
}
