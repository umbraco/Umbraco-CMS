import { UmbElementCurrentUserPermissionServerDataSource } from './element-current-user-permission.server.data-source.js';
import type { UmbElementCurrentUserPermissionsModel } from './types.js';
import { UmbRepositoryBase, type UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A repository for the current user's element permissions, as resolved by the server.
 * @class UmbElementCurrentUserPermissionRepository
 */
export class UmbElementCurrentUserPermissionRepository extends UmbRepositoryBase {
	#dataSource = new UmbElementCurrentUserPermissionServerDataSource(this);

	/**
	 * Requests the permissions the current user has at the given elements or element folders.
	 * @param {Array<string>} uniques - The uniques of the elements or element folders
	 * @returns {Promise<UmbRepositoryResponse<Array<UmbElementCurrentUserPermissionsModel>>>} The permissions for each unique
	 * @memberof UmbElementCurrentUserPermissionRepository
	 */
	async requestPermissions(
		uniques: Array<string>,
	): Promise<UmbRepositoryResponse<Array<UmbElementCurrentUserPermissionsModel>>> {
		if (uniques.length === 0) return { data: [] };
		return this.#dataSource.requestPermissions(uniques);
	}
}

export { UmbElementCurrentUserPermissionRepository as api };
