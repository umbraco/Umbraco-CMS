import type { UmbUserDetailModel } from '../../types.js';
import { UMB_USER_ENTITY_TYPE } from '../../entity.js';
import type { UmbUserCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type {
	DirectionModel,
	UserOrderModel,
	UserResponseModel,
	UserStateModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the user collection data from the server.
 * @class UmbUserCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbUserCollectionServerDataSource implements UmbCollectionDataSource<UmbUserDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUserCollectionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUserCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the user collection filtered by the given filter.
	 * @param {UmbUserCollectionFilterModel} filter
	 * @returns {*}
	 * @memberof UmbUserCollectionServerDataSource
	 */
	async getCollection(filter: UmbUserCollectionFilterModel) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			UserService.getFilterUser({
				filter: filter.filter,
				orderBy: filter.orderBy as unknown as UserOrderModel, // TODO: This is a temporary workaround to avoid a type error.
				orderDirection: filter.orderDirection as unknown as DirectionModel, // TODO: This is a temporary workaround to avoid a type error.
				skip: filter.skip,
				take: filter.take,
				userGroupIds: filter.userGroupIds,
				userStates: filter.userStates as unknown as Array<UserStateModel>, // TODO: This is a temporary workaround to avoid a type error.
			}),
		);

		if (data) {
			const { items, total } = data;

			const mappedItems: Array<UmbUserDetailModel> = items.map((item: UserResponseModel) => {
				const userDetail: UmbUserDetailModel = {
					entityType: UMB_USER_ENTITY_TYPE,
					email: item.email,
					userName: item.userName,
					name: item.name,
					userGroupUniques: item.userGroupIds.map((reference) => {
						return {
							unique: reference.id,
						};
					}),
					unique: item.id,
					languageIsoCode: item.languageIsoCode || null,
					documentStartNodeUniques: item.documentStartNodeIds.map((node) => {
						return {
							unique: node.id,
						};
					}),
					mediaStartNodeUniques: item.mediaStartNodeIds.map((node) => {
						return {
							unique: node.id,
						};
					}),
					hasDocumentRootAccess: item.hasDocumentRootAccess,
					hasMediaRootAccess: item.hasMediaRootAccess,
					avatarUrls: item.avatarUrls,
					state: item.state,
					failedLoginAttempts: item.failedLoginAttempts,
					createDate: item.createDate,
					updateDate: item.updateDate,
					lastLoginDate: item.lastLoginDate || null,
					lastLockoutDate: item.lastLockoutDate || null,
					lastPasswordChangeDate: item.lastPasswordChangeDate || null,
					isAdmin: item.isAdmin,
					kind: item.kind,
				};

				return userDetail;
			});

			return { data: { items: mappedItems, total } };
		}

		return { error };
	}
}
