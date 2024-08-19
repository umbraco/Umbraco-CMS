import { UMB_DICTIONARY_ENTITY_TYPE, UMB_DICTIONARY_ROOT_ENTITY_TYPE } from '../entity.js';
import type { UmbDictionaryTreeItemModel } from './types.js';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { NamedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * A data source for the Dictionary tree that fetches data from the server
 * @class UmbDictionaryTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbDictionaryTreeServerDataSource extends UmbTreeServerDataSourceBase<
	NamedEntityTreeItemResponseModel,
	UmbDictionaryTreeItemModel
> {
	/**
	 * Creates an instance of UmbDictionaryTreeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDictionaryTreeServerDataSource
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
	DictionaryService.getTreeDictionaryRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parent.unique === null) {
		return getRootItems(args);
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DictionaryService.getTreeDictionaryChildren({
			parentId: args.parent.unique,
			skip: args.skip,
			take: args.take,
		});
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	DictionaryService.getTreeDictionaryAncestors({
		descendantId: args.treeItem.unique,
	});

const mapper = (item: NamedEntityTreeItemResponseModel): UmbDictionaryTreeItemModel => {
	return {
		unique: item.id,
		parent: {
			unique: item.parent?.id || null,
			entityType: item.parent ? UMB_DICTIONARY_ENTITY_TYPE : UMB_DICTIONARY_ROOT_ENTITY_TYPE,
		},
		name: item.name,
		entityType: UMB_DICTIONARY_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		isFolder: false,
		icon: 'icon-book-alt',
	};
};
