import type { UmbUserGroupItemModel } from './types.js';
import type { UserGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UserGroupService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for User Group items
 * @class UmbUserGroupItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbUserGroupItemServerDataSource extends UmbItemServerDataSourceBase<
	UserGroupItemResponseModel,
	UmbUserGroupItemModel
> {
	/**
	 * Creates an instance of UmbUserGroupItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUserGroupItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => UserGroupService.getItemUserGroup({ id: uniques });

const mapper = (item: UserGroupItemResponseModel): UmbUserGroupItemModel => {
	return {
		unique: item.id,
		name: item.name,
		icon: item.icon || null,
	};
};
