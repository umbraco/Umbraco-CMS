import { type UmbEnableUserDataSource } from './types.js';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for enabling users
 * @export
 * @class UmbEnableUserServerDataSource
 */
export class UmbEnableUserServerDataSource implements UmbEnableUserDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbEnableUserServerDataSource.
	 * @param {UmbControllerHost} host
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

		return tryExecuteAndNotify(
			this.#host,
			UserResource.postUserEnable({
				requestBody: {
					userIds,
				},
			}),
		);
	}
}
