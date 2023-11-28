import { UMB_USER_ENTITY_TYPE, type UmbUserCollectionFilterModel, type UmbUserDetailModel } from '../../types.js';
import { UmbCollectionDataSource, extendDataSourcePagedResponseData } from '@umbraco-cms/backoffice/repository';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the user collection data from the server.
 * @export
 * @class UmbUserCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbUserCollectionServerDataSource implements UmbCollectionDataSource<UmbUserDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUserCollectionServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbUserCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the user collection filtered by the given filter.
	 * @param {UmbUserCollectionFilterModel} filter
	 * @return {*}
	 * @memberof UmbUserCollectionServerDataSource
	 */
	async getCollection(filter: UmbUserCollectionFilterModel) {
		const response = await tryExecuteAndNotify(this.#host, UserResource.getUserFilter(filter));
		return extendDataSourcePagedResponseData<UmbUserDetailModel>(response, {
			entityType: UMB_USER_ENTITY_TYPE,
		});
	}
}
