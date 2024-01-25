import type { UmbMemberTypeTreeItemModel } from './types.js';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { EntityTreeItemResponseModel} from '@umbraco-cms/backoffice/backend-api';
import { MemberTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the MemberType tree that fetches data from the server
 * @export
 * @class UmbMemberTypeTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbMemberTypeTreeServerDataSource extends UmbTreeServerDataSourceBase<
	EntityTreeItemResponseModel,
	UmbMemberTypeTreeItemModel
> {
	/**
	 * Creates an instance of UmbMemberTypeTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberTypeTreeServerDataSource
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
const getRootItems = () => MemberTypeResource.getTreeMemberTypeRoot({});

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		return getRootItems();
	} else {
		throw new Error('Not supported for the member type tree');
	}
};

const mapper = (item: EntityTreeItemResponseModel): UmbMemberTypeTreeItemModel => {
	return {
		id: item.id,
		parentId: item.parentId || null,
		name: item.name,
		entityType: 'member-type',
		hasChildren: item.hasChildren,
		isContainer: item.isContainer,
		isFolder: false,
	};
};
