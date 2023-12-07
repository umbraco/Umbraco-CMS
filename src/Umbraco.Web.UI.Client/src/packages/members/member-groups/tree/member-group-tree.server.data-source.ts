import { UmbMemberGroupTreeItemModel } from './types.js';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import { EntityTreeItemResponseModel, MemberGroupResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the MemberGroup tree that fetches data from the server
 * @export
 * @class UmbMemberGroupTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbMemberGroupTreeServerDataSource extends UmbTreeServerDataSourceBase<
	EntityTreeItemResponseModel,
	UmbMemberGroupTreeItemModel
> {
	/**
	 * Creates an instance of UmbMemberGroupTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberGroupTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getChildrenOf,
			mapper,
		});
	}
}

const getChildrenOf = () => {
	return MemberGroupResource.getTreeMemberGroupRoot({});
};

const mapper = (item: EntityTreeItemResponseModel): UmbMemberGroupTreeItemModel => {
	return {
		id: item.id!,
		parentId: item.parentId || null,
		name: item.name!,
		type: 'member-group',
		isContainer: item.isContainer!,
		hasChildren: item.hasChildren!,
	};
};
