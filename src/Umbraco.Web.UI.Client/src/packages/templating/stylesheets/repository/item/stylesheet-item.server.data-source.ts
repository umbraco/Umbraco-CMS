import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { StylesheetItemResponseModel, StylesheetResource } from '@umbraco-cms/backoffice/backend-api';

/**
 * A data source for stylesheet items that fetches data from the server
 * @export
 * @class UmbStylesheetItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbStylesheetItemServerDataSource implements UmbItemDataSource<StylesheetItemResponseModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbStylesheetItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbStylesheetItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the items for the given paths from the server
	 * @param {Array<string>} paths
	 * @return {*}
	 * @memberof UmbStylesheetItemServerDataSource
	 */
	async getItems(paths: Array<string>) {
		if (!paths) throw new Error('Paths are missing');

		return tryExecuteAndNotify(
			this.#host,
			StylesheetResource.getStylesheetItem({
				path: paths,
			}),
		);
	}
}
