import { UmbRelationTypeTreeItemModel } from './types.js';
import { EntityTreeItemResponseModel, RelationTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Relation Type tree that fetches data from the server
 * @export
 * @class UmbRelationTypeTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbRelationTypeTreeServerDataSource extends UmbTreeServerDataSourceBase<
	EntityTreeItemResponseModel,
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

// eslint-disable-next-line local-rules/no-direct-api-import
const getRootItems = () => RelationTypeResource.getTreeRelationTypeRoot({});

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		return getRootItems();
	} else {
		throw new Error('Not supported for the relation type tree');
	}
};

const mapper = (item: EntityTreeItemResponseModel): UmbRelationTypeTreeItemModel => {
	return {
		id: item.id,
		parentId: item.parentId || null,
		name: item.name,
		entityType: 'relation-type',
		hasChildren: item.hasChildren,
		isContainer: item.isContainer,
	};
};
