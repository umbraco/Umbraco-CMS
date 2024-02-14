import { UserGroupResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbUserGroupCollectionFilterModel } from '../types.js';
import type { UmbUserGroupDetailModel } from '../../types.js';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../../entity.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the UserGroup that fetches data from the server
 * @export
 * @class UmbUserGroupCollectionServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserGroupCollectionServerDataSource implements UmbCollectionDataSource<UmbUserGroupDetailModel> {
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
				const userGroup: UmbUserGroupDetailModel = {
					unique: item.id,
					entityType: UMB_USER_GROUP_ENTITY_TYPE,
					isSystemGroup: item.isSystemGroup,
					name: item.name,
					icon: item.icon || null,
					sections: item.sections,
					languages: item.languages,
					hasAccessToAllLanguages: item.hasAccessToAllLanguages,
					documentStartNode: item.documentStartNode ? { unique: item.documentStartNode.id } : null,
					documentRootAccess: item.documentRootAccess,
					mediaStartNode: item.mediaStartNode ? { unique: item.mediaStartNode.id } : null,
					mediaRootAccess: item.mediaRootAccess,
					permissions: item.permissions,
				};
				return userGroup;
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
