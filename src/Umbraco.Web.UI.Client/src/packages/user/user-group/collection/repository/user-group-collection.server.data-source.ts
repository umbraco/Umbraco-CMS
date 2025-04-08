import type { UmbUserGroupCollectionFilterModel } from '../types.js';
import type { UmbUserGroupDetailModel } from '../../types.js';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../../entity.js';
import { UserGroupService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the UserGroup that fetches data from the server
 * @class UmbUserGroupCollectionServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserGroupCollectionServerDataSource implements UmbCollectionDataSource<UmbUserGroupDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUserGroupCollectionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUserGroupCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async getCollection(filter: UmbUserGroupCollectionFilterModel) {
		const { data, error } = await tryExecute(
			this.#host,
			UserGroupService.getFilterUserGroup({ skip: filter.skip, take: filter.take, filter: filter.query }),
		);

		if (data) {
			const mappedItems = data.items.map((item) => {
				const userGroup: UmbUserGroupDetailModel = {
					alias: item.alias,
					aliasCanBeChanged: item.aliasCanBeChanged,
					documentRootAccess: item.documentRootAccess,
					documentStartNode: item.documentStartNode ? { unique: item.documentStartNode.id } : null,
					entityType: UMB_USER_GROUP_ENTITY_TYPE,
					fallbackPermissions: item.fallbackPermissions,
					hasAccessToAllLanguages: item.hasAccessToAllLanguages,
					icon: item.icon || null,
					isDeletable: item.isDeletable,
					languages: item.languages,
					mediaRootAccess: item.mediaRootAccess,
					mediaStartNode: item.mediaStartNode ? { unique: item.mediaStartNode.id } : null,
					name: item.name,
					permissions: item.permissions,
					sections: item.sections,
					unique: item.id,
				};
				return userGroup;
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
