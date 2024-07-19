import { UMB_MEMBER_TYPE_ENTITY_TYPE, UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import type { UmbMemberTypeTreeItemModel } from './types.js';
import type { UmbTreeChildrenOfRequestArgs, UmbTreeRootItemsRequestArgs } from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { NamedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the MemberType tree that fetches data from the server
 * @export
 * @class UmbMemberTypeTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbMemberTypeTreeServerDataSource extends UmbTreeServerDataSourceBase<
	NamedEntityTreeItemResponseModel,
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
			getAncestorsOf,
			mapper,
		});
	}
}

const getRootItems = (args: UmbTreeRootItemsRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	MemberTypeService.getTreeMemberTypeRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parent.unique === null) {
		return getRootItems(args);
	} else {
		throw new Error('Not supported for the member type tree');
	}
};

const getAncestorsOf = () => {
	throw new Error('Not supported for the member type tree');
};

const mapper = (item: NamedEntityTreeItemResponseModel): UmbMemberTypeTreeItemModel => {
	return {
		unique: item.id,
		parent: {
			unique: item.parent ? item.parent.id : null,
			entityType: item.parent ? UMB_MEMBER_TYPE_ENTITY_TYPE : UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE,
		},
		name: item.name,
		entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		isFolder: false,
		icon: 'icon-user',
	};
};
