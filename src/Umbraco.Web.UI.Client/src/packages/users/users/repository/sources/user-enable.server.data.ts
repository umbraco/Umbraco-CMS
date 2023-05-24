import { UmbUserEnableDataSource } from '../../types';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbUserEnableServerDataSource
 */
export class UmbUserEnableServerDataSource implements UmbUserEnableDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbUserEnableServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserEnableServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Set groups for users
	 * @param {Array<string>} id
	 * @return {*}
	 * @memberof UmbUserEnableServerDataSource
	 */
	async enable(userIds: string[]) {
		if (!userIds) throw new Error('User ids are missing');

		return tryExecuteAndNotify(
			this.#host,
			UserResource.postUserEnable({
				requestBody: {
					userIds,
				},
			})
		);
	}
}
