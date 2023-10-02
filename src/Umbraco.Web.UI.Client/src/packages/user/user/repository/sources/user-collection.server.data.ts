import type { UmbUserCollectionFilterModel, UmbUserDetail } from '../../types.js';
import { UmbCollectionDataSource, extendDataSourcePagedResponseData } from '@umbraco-cms/backoffice/repository';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the User that fetches data from the server
 * @export
 * @class UmbUserCollectionServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserCollectionServerDataSource implements UmbCollectionDataSource<UmbUserDetail> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbUserCollectionServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserCollectionServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	async getCollection() {
		const response = await tryExecuteAndNotify(this.#host, UserResource.getUser({}));
		return extendDataSourcePagedResponseData<UmbUserDetail>(response, {
			entityType: 'user',
		});
	}

	filterCollection(filter: UmbUserCollectionFilterModel) {
		return tryExecuteAndNotify(this.#host, UserResource.getUserFilter(filter));
		// TODO: Most likely missing the right type, and should then extend the data set with entityType.
	}
}
