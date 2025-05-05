import type { UmbUserGroupItemModel } from './types.js';
import type { UserGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UserGroupService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDataApiItemGetRequestController } from '@umbraco-cms/backoffice/entity-item';

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
			mapper,
		});
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const itemRequestManager = new UmbDataApiItemGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => UserGroupService.getItemUserGroup({ query: { id: args.uniques } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

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
