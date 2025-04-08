import type { UmbUnlockUserDataSource } from './types.js';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for unlocking users
 * @class UmbUnlockUserServerDataSource
 */
export class UmbUnlockUserServerDataSource implements UmbUnlockUserDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUnlockUserServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUnlockUserServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Unlock users
	 * @param {string[]} userIds
	 * @returns {Promise<void>}
	 * @memberof UmbUnlockUserServerDataSource
	 */
	async unlock(userIds: string[]) {
		if (!userIds) throw new Error('User ids are missing');

		return tryExecute(
			this.#host,
			UserService.postUserUnlock({
				requestBody: {
					userIds: userIds.map((id) => ({ id })),
				},
			}),
		);
	}
}
