import { type UmbUnlockUserDataSource } from './types.js';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for unlocking users
 * @export
 * @class UmbUnlockUserServerDataSource
 */
export class UmbUnlockUserServerDataSource implements UmbUnlockUserDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUnlockUserServerDataSource.
	 * @param {UmbControllerHost} host
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

		return tryExecuteAndNotify(
			this.#host,
			UserResource.postUserUnlock({
				requestBody: {
					userIds,
				},
			}),
		);
	}
}
