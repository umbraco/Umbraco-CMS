import type { UmbUserGroupCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UserGroupResource } from '@umbraco-cms/backoffice/backend-api';

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

	async getCollection(filter: UmbUserGroupCollectionFilterModel) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			UserGroupResource.getUserGroup({ skip: filter.skip, take: filter.take }),
		);

		if (data) {
			const mappedItems = data.items.map((item) => {
				return {
					...item,
				};
			});
		}

		return { error };
	}
}
