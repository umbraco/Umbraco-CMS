import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A server data source for Media Urls
 * @export
 * @class UmbMediaUrlServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbMediaUrlServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaUrlServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaUrlServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Publish one or more variants of a Document
	 * @param {string} unique
	 * @param {Array<UmbVariantId>} variantIds
	 * @return {*}
	 * @memberof UmbMediaUrlServerDataSource
	 */
	async getUrls(uniques: Array<string>) {
		if (!uniques) throw new Error('Id is missing');
		return tryExecuteAndNotify(this.#host, MediaService.getMediaUrls({ id: uniques }));
	}
}
