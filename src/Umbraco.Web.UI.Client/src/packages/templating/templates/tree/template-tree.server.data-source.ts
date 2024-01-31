import { UMB_TEMPLATE_ENTITY_TYPE } from '../entity.js';
import type { UmbTemplateTreeItemModel } from './types.js';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { NamedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { TemplateResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Template tree that fetches data from the server
 * @export
 * @class UmbTemplateTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbTemplateTreeServerDataSource extends UmbTreeServerDataSourceBase<
	NamedEntityTreeItemResponseModel,
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
		unique: item.id,
		parentUnique: item.parent ? item.parent.id : null,
		name: item.name,
		entityType: UMB_TEMPLATE_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		isFolder: false,
	};
};
