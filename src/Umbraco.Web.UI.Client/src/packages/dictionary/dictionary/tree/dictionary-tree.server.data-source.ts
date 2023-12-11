import { UmbDictionaryTreeItemModel } from './types.js';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DictionaryResource, EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

/**
 * A data source for the Dictionary tree that fetches data from the server
 * @export
 * @class UmbDictionaryTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbDictionaryTreeServerDataSource extends UmbTreeServerDataSourceBase<
	EntityTreeItemResponseModel,
	UmbDictionaryTreeItemModel
> {
	/**
	 * Creates an instance of UmbDictionaryTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDictionaryTreeServerDataSource
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
const getRootItems = () => DictionaryResource.getTreeDictionaryRoot({});

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		return getRootItems();
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DictionaryResource.getTreeDictionaryChildren({
			parentId: parentUnique,
		});
	}
};

const mapper = (item: EntityTreeItemResponseModel): UmbDictionaryTreeItemModel => {
	return {
		id: item.id,
		parentId: item.parentId || null,
		name: item.name,
		entityType: 'dictionary-item',
		hasChildren: item.hasChildren,
		isContainer: item.isContainer,
		isFolder: false,
	};
};
