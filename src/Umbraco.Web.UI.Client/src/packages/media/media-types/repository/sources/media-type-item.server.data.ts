import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { MediaTypeItemResponseModel, MediaTypeResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Media Type items that fetches data from the server
 * @export
 * @class UmbMediaTypeItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbMediaTypeItemServerDataSource implements UmbItemDataSource<MediaTypeItemResponseModel> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbMediaTypeItemServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaTypeItemServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbMediaTypeItemServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		return tryExecuteAndNotify(
			this.#host,
			MediaTypeResource.getMediaTypeItem({
				id: ids,
			}),
		);
	}
}
