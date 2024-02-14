import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import type { UmbDictionaryTreeItemModel } from './types.js';
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

const mapper = (item: NamedEntityTreeItemResponseModel): UmbDictionaryTreeItemModel => {
	return {
		unique: item.id,
		parentUnique: item.parent?.id || null,
		name: item.name,
		entityType: UMB_DICTIONARY_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		isFolder: false,
	};
};
