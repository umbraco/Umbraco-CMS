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
			getRootItems,
			getChildrenOf,
			mapper,
		});
	}
}

// eslint-disable-next-line local-rules/no-direct-api-import
const getRootItems = () => MemberGroupResource.getTreeMemberGroupRoot({});

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		return getRootItems();
	} else {
		throw new Error('Not supported for the member group tree');
	}
};

const mapper = (item: EntityTreeItemResponseModel): UmbMemberGroupTreeItemModel => {
	return {
		id: item.id,
		parentId: item.parentId || null,
		name: item.name,
		entityType: 'member-group',
		isContainer: item.isContainer,
		hasChildren: item.hasChildren,
		isFolder: false,
	};
};
