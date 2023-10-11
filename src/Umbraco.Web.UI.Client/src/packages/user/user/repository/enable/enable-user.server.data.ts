import { type UmbEnableUserDataSource } from './types.js';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class enable
 */
export class UmbEnableUserServerDataSource implements UmbEnableUserDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbEnableUserServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbEnableUserServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
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
