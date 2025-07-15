import type { UmbDisableUserDataSource } from './types.js';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for disabling users
 * @class UmbDisableUserServerDataSource
 */
export class UmbDisableUserServerDataSource implements UmbDisableUserDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDisableUserServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDisableUserServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Disables the specified user ids
	 * @param {string[]} userIds
	 * @returns {Promise<void>}
	 * @memberof UmbDisableUserServerDataSource
	 */
	async disable(userIds: string[]) {
		if (!userIds) throw new Error('User ids are missing');

		return tryExecute(
			this.#host,
			UserService.postUserDisable({
				body: {
					userIds: userIds.map((id) => ({ id })),
				},
			}),
		);
	}
}
