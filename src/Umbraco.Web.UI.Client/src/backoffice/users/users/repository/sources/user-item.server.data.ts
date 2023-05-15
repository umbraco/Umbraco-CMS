import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UserItemResponseModel, UserResource } from '@umbraco-cms/backoffice/backend-api';

/**
 * A data source for user items that fetches data from the server
 * @export
 * @class UmbUserItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbUserItemServerDataSource implements UmbItemDataSource<UserItemResponseModel> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbUserItemServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserItemServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
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
