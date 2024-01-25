import type { UmbTemplateTreeItemModel } from './types.js';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type {
	EntityTreeItemResponseModel,
	NamedEntityTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { TemplateResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Template tree that fetches data from the server
 * @export
 * @class UmbTemplateTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbTemplateTreeServerDataSource extends UmbTreeServerDataSourceBase<
	EntityTreeItemResponseModel,
	UmbTemplateTreeItemModel
> {
	/**
	 * Creates an instance of UmbTemplateTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbTemplateTreeServerDataSource
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
const getRootItems = () => TemplateResource.getTreeTemplateRoot({});

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		return getRootItems();
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return TemplateResource.getTreeTemplateChildren({
			parentId: parentUnique,
		});
	}
};

const mapper = (item: NamedEntityTreeItemResponseModel): UmbTemplateTreeItemModel => {
	return {
		id: item.id,
		parentId: item.parent ? item.parent.id : null,
		name: item.name,
		entityType: 'template',
		hasChildren: item.hasChildren,
		isFolder: false,
	};
};
