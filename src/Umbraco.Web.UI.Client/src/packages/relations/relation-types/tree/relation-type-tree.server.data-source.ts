import type { UmbRelationTypeTreeItemModel } from './types.js';
import type { NamedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { RelationTypeResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbTreeChildrenOfRequestArgs, UmbTreeRootItemsRequestArgs } from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Relation Type tree that fetches data from the server
 * @export
 * @class UmbRelationTypeTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbRelationTypeTreeServerDataSource extends UmbTreeServerDataSourceBase<
	NamedEntityTreeItemResponseModel,
	UmbRelationTypeTreeItemModel
> {
	/**
	 * Creates an instance of UmbRelationTypeTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbRelationTypeTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems,
			getChildrenOf,
			mapper,
		});
	}
}

const getRootItems = (args: UmbTreeRootItemsRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	RelationTypeResource.getTreeRelationTypeRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parentUnique === null) {
		return getRootItems(args);
	} else {
		throw new Error('Not supported for the relation type tree');
	}
};

const mapper = (item: NamedEntityTreeItemResponseModel): UmbRelationTypeTreeItemModel => {
	return {
		unique: item.id,
		parentUnique: item.parent ? item.parent.id : null,
		name: item.name,
		entityType: 'relation-type',
		hasChildren: item.hasChildren,
		isFolder: false,
	};
};
