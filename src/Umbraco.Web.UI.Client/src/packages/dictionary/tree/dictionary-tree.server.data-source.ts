import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import type { UmbDictionaryTreeItemModel } from './types.js';
import type { UmbTreeChildrenOfRequestArgs, UmbTreeRootItemsRequestArgs } from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { NamedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DictionaryResource } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * A data source for the Dictionary tree that fetches data from the server
 * @export
 * @class UmbDictionaryTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbDictionaryTreeServerDataSource extends UmbTreeServerDataSourceBase<
	NamedEntityTreeItemResponseModel,
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

const getRootItems = (args: UmbTreeRootItemsRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	DictionaryResource.getTreeDictionaryRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parentUnique === null) {
		return getRootItems(args);
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DictionaryResource.getTreeDictionaryChildren({
			parentId: args.parentUnique,
		});
	}
};

const mapper = (item: NamedEntityTreeItemResponseModel): UmbDictionaryTreeItemModel => {
	return {
		unique: item.id,
		parentUnique: item.parent?.id || null,
		name: item.name,
		entityType: UMB_DICTIONARY_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		isFolder: false,
		icon: 'icon-book-alt',
	};
};
