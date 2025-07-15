import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../../entity.js';
import type { UmbMemberGroupItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { MemberGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberGroupService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemDataApiGetRequestController } from '@umbraco-cms/backoffice/entity-item';

/**
 * A server data source for Member Group items
 * @class UmbMemberGroupItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbMemberGroupItemServerDataSource extends UmbItemServerDataSourceBase<
	MemberGroupItemResponseModel,
	UmbMemberGroupItemModel
> {
	/**
	 * Creates an instance of UmbMemberGroupItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberGroupItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			mapper,
		});
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const itemRequestManager = new UmbItemDataApiGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => MemberGroupService.getItemMemberGroup({ query: { id: args.uniques } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: MemberGroupItemResponseModel): UmbMemberGroupItemModel => {
	return {
		unique: item.id,
		name: item.name,
		entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
	};
};
