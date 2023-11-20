import type { UmbUserGroupCollectionFilterModel } from '../../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UserGroupResponseModel, UserGroupResource } from '@umbraco-cms/backoffice/backend-api';

/**
 * A data source for the UserGroup that fetches data from the server
 * @export
 * @class UmbUserGroupCollectionServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserGroupCollectionServerDataSource implements UmbCollectionDataSource<UserGroupResponseModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUserGroupCollectionServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbUserGroupCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	getCollection(filter: UmbUserGroupCollectionFilterModel) {
		// TODO: Switch this to the filter endpoint when available
		return tryExecuteAndNotify(this.#host, UserGroupResource.getUserGroup({}));
	}
}
