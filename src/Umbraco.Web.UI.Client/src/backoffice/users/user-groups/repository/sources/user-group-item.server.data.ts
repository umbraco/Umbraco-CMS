import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UserGroupItemResponseModel, UserGroupResource } from '@umbraco-cms/backoffice/backend-api';

/**
 * A data source for user group items that fetches data from the server
 * @export
 * @class UmbUserGroupItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbUserGroupItemServerDataSource implements UmbItemDataSource<UserGroupItemResponseModel> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbUserGroupItemServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserGroupItemServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbUserGroupItemServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');

		return tryExecuteAndNotify(
			this.#host,
			UserGroupResource.getUserGroupItem({
				id: ids,
			})
		);
	}
}
