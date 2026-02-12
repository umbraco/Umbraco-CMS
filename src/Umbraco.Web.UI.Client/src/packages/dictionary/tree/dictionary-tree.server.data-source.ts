import { UMB_DICTIONARY_ENTITY_TYPE, UMB_DICTIONARY_ROOT_ENTITY_TYPE } from '../entity.js';
import type { UmbDictionaryTreeItemModel } from './types.js';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import type { UmbOffsetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';
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

const getRootItems = async (args: UmbTreeRootItemsRequestArgs) => {
	const { skip = 0, take = 100 } = (args.paging ?? {}) as UmbOffsetPaginationRequestModel;
	// eslint-disable-next-line local-rules/no-direct-api-import
	const { data, ...rest } = await DictionaryService.getTreeDictionaryRoot({
		query: { skip, take },
	});
	return { data: { ...data, totalBefore: 0, totalAfter: Math.max(data.total - data.items.length, 0) }, ...rest };
};

const getChildrenOf = async (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parent.unique === null) {
		return getRootItems(args);
	} else {
		const { skip = 0, take = 100 } = (args.paging ?? {}) as UmbOffsetPaginationRequestModel;
		// eslint-disable-next-line local-rules/no-direct-api-import
		const { data, ...rest } = await DictionaryService.getTreeDictionaryChildren({
			query: { parentId: args.parent.unique, skip, take },
		});
		return { data: { ...data, totalBefore: 0, totalAfter: Math.max(data.total - data.items.length, 0) }, ...rest };
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	DictionaryService.getTreeDictionaryAncestors({
		query: { descendantId: args.treeItem.unique },
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
