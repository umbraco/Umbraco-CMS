import type { UmbUserGroupItemModel } from './types.js';
import { UmbManagementApiUserGroupItemDataRequestManager } from './user-group-item.server.request-manager.js';
import type { UserGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
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
	#itemRequestManager = new UmbManagementApiUserGroupItemDataRequestManager(this);

	/**
	 * Creates an instance of UmbUserGroupItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUserGroupItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			mapper,
		});
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const { data, error } = await this.#itemRequestManager.getItems(uniques);

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: UserGroupItemResponseModel): UmbUserGroupItemModel => {
	return {
		unique: item.id,
		name: item.name,
		icon: item.icon || null,
	};
};
