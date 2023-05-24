import type { UmbUserCollectionFilterModel } from '../../types.js';
import { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';
import { UserResponseModel, UserResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the User that fetches data from the server
 * @export
 * @class UmbUserCollectionServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserCollectionServerDataSource implements UmbCollectionDataSource<UserResponseModel> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbUserCollectionServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserCollectionServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	getCollection() {
		return tryExecuteAndNotify(this.#host, UserResource.getUser({}));
	}

	filterCollection(filter: UmbUserCollectionFilterModel) {
		return tryExecuteAndNotify(this.#host, UserResource.getUserFilter(filter));
	}
}
