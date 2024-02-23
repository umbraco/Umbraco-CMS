import type { UmbUserDetailModel } from '../../types.js';
import { UMB_USER_ENTITY_TYPE } from '../../entity.js';
import type { UmbUserCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UserResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UserResource } from '@umbraco-cms/backoffice/external/backend-api';
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
		const { data, error } = await tryExecuteAndNotify(this.#host, UserResource.getFilterUser(filter));

		if (data) {
			const { items, total } = data;

			const mappedItems: Array<UmbUserDetailModel> = items.map((item: UserResponseModel) => {
				const userDetail: UmbUserDetailModel = {
					entityType: UMB_USER_ENTITY_TYPE,
					email: item.email,
					userName: item.userName,
					name: item.name,
					userGroupUniques: item.userGroupIds,
					unique: item.id,
					languageIsoCode: item.languageIsoCode || null,
					documentStartNodeUniques: item.documentStartNodeIds,
					mediaStartNodeUniques: item.mediaStartNodeIds,
					avatarUrls: item.avatarUrls,
					state: item.state,
					failedLoginAttempts: item.failedLoginAttempts,
					createDate: item.createDate,
					updateDate: item.updateDate,
					lastLoginDate: item.lastLoginDate || null,
					lastLockoutDate: item.lastLockoutDate || null,
					lastPasswordChangeDate: item.lastPasswordChangeDate || null,
				};

				return userDetail;
			});

			return { data: { items: mappedItems, total } };
		}

		return { error };
	}
}
