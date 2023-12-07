import { UmbTemplateTreeItemModel } from './types.js';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import { EntityTreeItemResponseModel, TemplateResource } from '@umbraco-cms/backoffice/backend-api';
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
			getChildrenOf,
			mapper,
		});
	}
}

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return TemplateResource.getTreeTemplateRoot({});
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return TemplateResource.getTreeTemplateChildren({
			parentId: parentUnique,
		});
	}
};

const mapper = (item: EntityTreeItemResponseModel): UmbTemplateTreeItemModel => {
	return {
		id: item.id!,
		parentId: item.parentId || null,
		name: item.name!,
		type: 'template',
		isContainer: item.isContainer!,
		hasChildren: item.hasChildren!,
	};
};
