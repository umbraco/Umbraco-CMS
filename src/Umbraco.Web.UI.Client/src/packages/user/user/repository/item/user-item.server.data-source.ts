import { UMB_USER_ENTITY_TYPE } from '../../entity.js';
import type { UmbUserItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UserItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for user items that fetches data from the server
 * @class UmbUserItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbUserItemServerDataSource extends UmbItemServerDataSourceBase<UserItemResponseModel, UmbUserItemModel> {
	/**
	 * Creates an instance of UmbUserItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUserItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => UserService.getItemUser({ query: { id: uniques } });

const mapper = (item: UserItemResponseModel): UmbUserItemModel => {
	return {
		avatarUrls: item.avatarUrls,
		entityType: UMB_USER_ENTITY_TYPE,
		name: item.name,
		unique: item.id,
		kind: item.kind,
	};
};
