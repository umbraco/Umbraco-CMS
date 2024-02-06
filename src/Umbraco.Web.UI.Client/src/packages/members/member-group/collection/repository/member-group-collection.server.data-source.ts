import type { UmbMemberGroupCollectionFilterModel, UmbMemberGroupCollectionModel } from '../types.js';
import type { UmbMemberGroupDetailModel } from '../../types.js';
import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../../entity.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the member collection data from the server.
 * @export
 * @class UmbMemberGroupCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbMemberGroupCollectionServerDataSource implements UmbCollectionDataSource<UmbMemberGroupDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberGroupCollectionServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberGroupCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the member collection filtered by the given filter.
	 * @param {UmbMemberGroupCollectionFilterModel} filter
	 * @return {*}
	 * @memberof UmbMemberGroupCollectionServerDataSource
	 */
	async getCollection(filter: UmbMemberGroupCollectionFilterModel) {
		//const { data, error } = await tryExecuteAndNotify(this.#host, MemberGroupResource.getCollectionMemberGroup(filter));
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/member-group/collection'),
		);

		if (data) {
			const items = data.items.map((item) => {
				const model: UmbMemberGroupCollectionModel = {
					unique: item.isoCode,
					name: item.name,
					entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
				};

				return model;
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
