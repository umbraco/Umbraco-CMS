import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Data Type items that fetches data from the server
 * @class UmbUserSetGroupsServerDataSource
 */
export class UmbUserSetGroupsServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUserSetGroupsServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUserSetGroupsServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Set groups for users
	 * @param {Array<string>} id
	 * @param userIds
	 * @param userGroupIds
	 * @returns {*}
	 * @memberof UmbUserSetGroupsServerDataSource
	 */
	async setGroups(userIds: string[], userGroupIds: string[]) {
		if (!userIds) throw new Error('User ids are missing');
		if (!userGroupIds) throw new Error('User group ids are missing');

		return tryExecuteAndNotify(
			this.#host,
			UserService.postUserSetUserGroups({
				requestBody: {
					userIds: userIds.map((id) => ({ id })),
					userGroupIds: userGroupIds.map((id) => ({ id })),
				},
			}),
		);
	}
}
