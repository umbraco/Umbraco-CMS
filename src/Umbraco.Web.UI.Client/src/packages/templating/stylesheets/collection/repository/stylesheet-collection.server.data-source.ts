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

	/**
	 * Creates an instance of UmbStylesheetCollectionServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbStylesheetCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	getCollection(filter: UmbStylesheetCollectionFilterModel) {
		return tryExecuteAndNotify(this.#host, StylesheetResource.getStylesheetAll(filter));
	}
}
