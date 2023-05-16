import { UmbUserSetGroupDataSource } from '../../types';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbUserSetGroupsServerDataSource
 */
export class UmbUserSetGroupsServerDataSource implements UmbUserSetGroupDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbUserSetGroupsServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserSetGroupsServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Set groups for users
	 * @param {Array<string>} id
	 * @return {*}
	 * @memberof UmbUserSetGroupsServerDataSource
	 */
	async setGroups(userIds: string[], userGroupIds: string[]) {
		if (!userIds) throw new Error('User ids are missing');
		if (!userGroupIds) throw new Error('User group ids are missing');

		return tryExecuteAndNotify(
			this.#host,
			UserResource.postUserSetUserGroups({
				requestBody: {
					userIds,
					userGroupIds,
				},
			})
		);
	}
}
