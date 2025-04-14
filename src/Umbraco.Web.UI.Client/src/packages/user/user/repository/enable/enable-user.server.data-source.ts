import type { UmbEnableUserDataSource } from './types.js';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for enabling users
 * @class UmbEnableUserServerDataSource
 */
export class UmbEnableUserServerDataSource implements UmbEnableUserDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbEnableUserServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbEnableUserServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Enables the specified user ids
	 * @param {string[]} userIds
	 * @returns {Promise<void>}
	 * @memberof UmbEnableUserServerDataSource
	 */
	async enable(userIds: string[]) {
		if (!userIds) throw new Error('User ids are missing');

		return tryExecute(
			this.#host,
			UserService.postUserEnable({
				requestBody: {
					userIds: userIds.map((id) => ({ id })),
				},
			}),
		);
	}
}
