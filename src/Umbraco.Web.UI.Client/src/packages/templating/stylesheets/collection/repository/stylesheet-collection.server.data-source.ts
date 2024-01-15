import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import { UmbStylesheetCollectionFilterModel, UmbStylesheetCollectionItemModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { StylesheetResource } from '@umbraco-cms/backoffice/backend-api';

/**
 * A data source for the Stylesheet collection that fetches data from the server
 * @export
 * @class UmbStylesheetCollectionServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbStylesheetCollectionServerDataSource
	implements UmbCollectionDataSource<UmbStylesheetCollectionItemModel>
{
	#host: UmbControllerHost;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	/**
	 * Creates an instance of UmbStylesheetCollectionServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbStylesheetCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the stylesheet collection items from the server
	 * @param {UmbStylesheetCollectionFilterModel} filter
	 * @return {*}
	 * @memberof UmbStylesheetCollectionServerDataSource
	 */
	async getCollection(filter: UmbStylesheetCollectionFilterModel) {
		const { data, error } = await tryExecuteAndNotify(this.#host, StylesheetResource.getStylesheetOverview(filter));

		if (data) {
			const items: Array<UmbStylesheetCollectionItemModel> = data.items.map((item) => {
				return {
					name: item.name,
					unique: this.#serverFilePathUniqueSerializer.toUnique(item.path),
				};
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
