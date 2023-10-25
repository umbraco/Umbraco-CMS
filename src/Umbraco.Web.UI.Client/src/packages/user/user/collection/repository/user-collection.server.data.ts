import { USER_ENTITY_TYPE, type UmbUserCollectionFilterModel, type UmbUserDetail } from '../../types.js';
import { UmbCollectionDataSource, extendDataSourcePagedResponseData } from '@umbraco-cms/backoffice/repository';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the user collection data from the server.
 * @export
 * @class UmbUserCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
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

	/**
	 * Gets the user collection from the server.
	 * @return {*}
	 * @memberof UmbUserCollectionServerDataSource
	 */
	async getCollection() {
		const response = await tryExecuteAndNotify(this.#host, UserResource.getUser({}));
		return extendDataSourcePagedResponseData<UmbUserDetail>(response, {
			entityType: USER_ENTITY_TYPE,
		});
	}

	/**
	 * Gets the user collection filtered by the given filter.
	 * @param {UmbUserCollectionFilterModel} filter
	 * @return {*}
	 * @memberof UmbUserCollectionServerDataSource
	 */
	async filterCollection(filter: UmbUserCollectionFilterModel) {
		const response = await tryExecuteAndNotify(this.#host, UserResource.getUserFilter(filter));
		return extendDataSourcePagedResponseData<UmbUserDetail>(response, {
			entityType: USER_ENTITY_TYPE,
		});
	}
}
