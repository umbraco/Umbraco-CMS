import { UmbDisableUserDataSource } from '../../types.js';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbDisableUserServerDataSource
 */
export class UmbDisableUserServerDataSource implements UmbDisableUserDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbDisableUserServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDisableUserServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
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
