import type { UmbMemberGroupCollectionFilterModel, UmbMemberGroupCollectionItemModel } from '../types.js';
import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../../entity.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { MemberGroupResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberGroupService } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * A data source that fetches the member group collection data from the server.
 * @class UmbMemberGroupCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbMemberGroupCollectionServerDataSource implements UmbCollectionDataSource<UmbMemberGroupCollectionItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberGroupCollectionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberGroupCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the member group collection filtered by the given filter.
	 * @param {UmbMemberGroupCollectionFilterModel} filter
	 * @returns {*}
	 * @memberof UmbMemberGroupCollectionServerDataSource
	 */
	async getCollection(query: UmbMemberGroupCollectionFilterModel) {
		const { data, error } = await tryExecute(
			this.#host,
			MemberGroupService.getMemberGroup({ query: { skip: query.skip, take: query.take } }),
		);

		if (error) {
			return { error };
		}

		if (!data) {
			return { data: { items: [], total: 0 } };
		}

		const { items, total } = data;

		const mappedItems: Array<UmbMemberGroupCollectionItemModel> = items.map((item: MemberGroupResponseModel) => {
			return {
				entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
				unique: item.id,
				name: item.name,
				icon: 'icon-users',
			};
		});

		return { data: { items: mappedItems, total } };
	}
}
