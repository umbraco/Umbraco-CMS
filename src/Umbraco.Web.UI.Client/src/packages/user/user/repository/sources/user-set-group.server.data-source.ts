import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

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
	 * @param {Array<string>} userIds - The ids of the users to set groups for
	 * @param {Array<string>} userGroupIds - The ids of the groups to set for the users
	 * @returns {Promise<UmbDataSourceErrorResponse>} The result of setting the groups
	 * @memberof UmbUserSetGroupsServerDataSource
	 */
	async setGroups(userIds: string[], userGroupIds: string[]): Promise<UmbDataSourceErrorResponse> {
		if (!userIds) throw new Error('User ids are missing');
		if (!userGroupIds) throw new Error('User group ids are missing');

		return tryExecute(
			this.#host,
			UserService.postUserSetUserGroups({
				body: {
					userIds: userIds.map((id) => ({ id })),
					userGroupIds: userGroupIds.map((id) => ({ id })),
				},
			}),
		);
	}
}
