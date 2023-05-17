import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { DataTypeItemResponseModel, DataTypeResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbDataTypeItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDataTypeItemServerDataSource implements UmbItemDataSource<DataTypeItemResponseModel> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbDataTypeItemServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDataTypeItemServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbDataTypeItemServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getDataTypeItem({
				id: ids,
			})
		);
	}
}
