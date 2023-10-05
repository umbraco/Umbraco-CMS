import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { MediaResource, MediaItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Media items that fetches data from the server
 * @export
 * @class UmbMediaItemServerDataSource
 * @implements {MediaItemDataSource}
 */
export class UmbMediaItemServerDataSource implements UmbItemDataSource<MediaItemResponseModel> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbMediaItemServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaItemServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @memberof UmbMediaItemServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		return tryExecuteAndNotify(
			this.#host,
			MediaResource.getMediaItem({
				id: ids,
			}),
		);
	}
}
