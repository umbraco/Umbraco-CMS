import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbUserDisableDataSource } from '../../types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbUserDisableServerDataSource
 */
export class UmbUserDisableServerDataSource implements UmbUserDisableDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbUserDisableServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserDisableServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Set groups for users
	 * @param {Array<string>} id
	 * @return {*}
	 * @memberof UmbUserDisableServerDataSource
	 */
	async disable(userIds: string[]) {
		if (!userIds) throw new Error('User ids are missing');

		return tryExecuteAndNotify(
			this.#host,
			UserResource.postUserDisable({
				requestBody: {
					userIds,
				},
			})
		);
	}
}
