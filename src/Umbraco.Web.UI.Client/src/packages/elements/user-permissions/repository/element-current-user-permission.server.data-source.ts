import type { UmbElementCurrentUserPermissionsModel } from './types.js';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A server data source for the current user's element permissions.
 * @class UmbElementCurrentUserPermissionServerDataSource
 */
export class UmbElementCurrentUserPermissionServerDataSource {
	readonly #host: UmbControllerHost;

	/**
	 * Creates an instance of UmbElementCurrentUserPermissionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementCurrentUserPermissionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Requests the permissions the current user has at the given elements or element folders, as resolved by the server.
	 * @param {Array<string>} uniques - The uniques of the elements or element folders
	 * @returns {Promise<UmbDataSourceResponse<Array<UmbElementCurrentUserPermissionsModel>>>} The permissions for each unique
	 * @memberof UmbElementCurrentUserPermissionServerDataSource
	 */
	async requestPermissions(
		uniques: Array<string>,
	): Promise<UmbDataSourceResponse<Array<UmbElementCurrentUserPermissionsModel>>> {
		const { data, error } = await tryExecute(
			this.#host,
			UserService.getUserCurrentPermissionsElement({ query: { id: uniques } }),
			{ disableNotifications: true },
		);

		if (error || !data) {
			return { error };
		}

		return {
			data: data.permissions.map((item) => ({ unique: item.nodeKey, verbs: item.permissions })),
		};
	}
}
