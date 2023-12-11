import { type UmbUserCollectionFilterModel, type UmbUserDetailModel } from '../../types.js';
import { UMB_USER_ENTITY_TYPE } from '../../entity.js';
import { UmbCollectionDataSource, extendDataSourcePagedResponseData } from '@umbraco-cms/backoffice/repository';
import { UserResource, UserResponseModel } from '@umbraco-cms/backoffice/backend-api';
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
		const { data, error } = await tryExecuteAndNotify(this.#host, UserResource.getUserFilter(filter));

		if (error) {
			return { error };
		}

		const { items, totalItems } = data;

		const mappedItems: Array<UmbUserDetailModel> = items.map((item: UserResponseModel) => {
			const userDetail: UmbUserDetailModel = {
				entityType: UMB_USER_ENTITY_TYPE,
				...item,
			};

			return userDetail;
		});

		return { items: mappedItems, totalItems };
	}
}
