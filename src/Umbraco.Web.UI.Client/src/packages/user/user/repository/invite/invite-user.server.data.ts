import { type UmbInviteUserDataSource } from './types.js';
import { InviteUserRequestModel, UserResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for inviting users
 * @export
 * @class UmbInviteUserServerDataSource
 */
export class UmbInviteUserServerDataSource implements UmbInviteUserDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbInviteUserServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbInviteUserServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	async invite(userId: string, requestBody: InviteUserRequestModel) {
		if (!userId) throw new Error('User id is missing');

		return tryExecuteAndNotify(
			this.#host,
			UserResource.postUserInvite({
				requestBody,
			}),
		);
	}
}
