import { type UmbDisableUserDataSource } from './types.js';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for disabling users
 * @export
 * @class UmbDisableUserServerDataSource
 */
export class UmbDisableUserServerDataSource implements UmbDisableUserDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDisableUserServerDataSource.
	 * @param {UmbControllerHost} host
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

		return tryExecuteAndNotify(
			this.#host,
			UserResource.postUserDisable({
				requestBody: {
					userIds,
				},
			}),
		);
	}
}
