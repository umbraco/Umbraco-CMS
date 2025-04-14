import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbMemberTypeItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { MemberTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Member Type items
 * @class UmbMemberTypeItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbMemberTypeItemServerDataSource extends UmbItemServerDataSourceBase<
	MemberTypeItemResponseModel,
	UmbMemberTypeItemModel
> {
	/**
	 * Creates an instance of UmbMemberTypeItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberTypeItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => MemberTypeService.getItemMemberType({ query: { id: uniques } });

const mapper = (item: MemberTypeItemResponseModel): UmbMemberTypeItemModel => {
	return {
		entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
		unique: item.id,
		name: item.name,
		icon: item.icon || '',
	};
};
