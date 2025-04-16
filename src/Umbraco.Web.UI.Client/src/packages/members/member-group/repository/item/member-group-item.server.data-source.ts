import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../../entity.js';
import type { UmbMemberGroupItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { MemberGroupItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberGroupService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

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
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => MemberGroupService.getItemMemberGroup({ id: uniques });

const mapper = (item: MemberGroupItemResponseModel): UmbMemberGroupItemModel => {
	return {
		unique: item.id,
		name: item.name,
		entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
	};
};
