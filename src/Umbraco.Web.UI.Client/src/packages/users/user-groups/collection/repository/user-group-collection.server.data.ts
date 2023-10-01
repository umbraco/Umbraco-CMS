import type { UmbUserGroupCollectionFilterModel } from '../../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UserGroupResponseModel, UserGroupResource } from '@umbraco-cms/backoffice/backend-api';

/**
 * A data source for the UserGroup that fetches data from the server
 * @export
 * @class UmbUserGroupCollectionServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserGroupCollectionServerDataSource implements UmbCollectionDataSource<UserGroupResponseModel> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbUserGroupCollectionServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbUserGroupCollectionServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	getCollection() {
		return tryExecuteAndNotify(this.#host, UserGroupResource.getUserGroup({}));
	}

	filterCollection(filter: UmbUserGroupCollectionFilterModel) {
		// TODO: Switch this to the filter endpoint when available
		return tryExecuteAndNotify(this.#host, UserGroupResource.getUserGroup({}));
	}
}
