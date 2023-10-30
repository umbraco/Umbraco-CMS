import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost} from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UserItemResponseModel, UserResource } from '@umbraco-cms/backoffice/backend-api';

/**
 * A data source for user items that fetches data from the server
 * @export
 * @class UmbUserItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbUserItemServerDataSource implements UmbItemDataSource<UserItemResponseModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUserItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbUserItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbUserItemServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');

		return tryExecuteAndNotify(
			this.#host,
			UserResource.getUserItem({
				id: ids,
			})
		);
	}
}
