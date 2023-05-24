import { UmbUserUnlockDataSource } from '../../types';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbUserUnlockServerDataSource
 */
export class UmbUserUnlockServerDataSource implements UmbUserUnlockDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbUserUnlockServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserUnlockServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * unlock users
	 * @param {Array<string>} id
	 * @return {*}
	 * @memberof UmbUserUnlockServerDataSource
	 */
	async unlock(userIds: string[]) {
		if (!userIds) throw new Error('User ids are missing');

		return tryExecuteAndNotify(
			this.#host,
			UserResource.postUserUnlock({
				requestBody: {
					userIds,
				},
			})
		);
	}
}
