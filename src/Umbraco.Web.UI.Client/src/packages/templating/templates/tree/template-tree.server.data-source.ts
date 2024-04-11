import { UMB_TEMPLATE_ENTITY_TYPE } from '../entity.js';
import type { UmbTemplateTreeItemModel } from './types.js';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { NamedEntityTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { TemplateService } from '@umbraco-cms/backoffice/external/backend-api';
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
			getAncestorsOf,
			mapper,
		});
	}
}

const getRootItems = (args: UmbTreeRootItemsRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	TemplateService.getTreeTemplateRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parentUnique === null) {
		return getRootItems(args);
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return TemplateService.getTreeTemplateChildren({
			parentId: args.parentUnique,
			skip: args.skip,
			take: args.take,
		});
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	TemplateService.getTreeTemplateAncestors({
		descendantId: args.descendantUnique,
	});

const mapper = (item: NamedEntityTreeItemResponseModel): UmbTemplateTreeItemModel => {
	return {
		unique: item.id,
		parentUnique: item.parent ? item.parent.id : null,
		name: item.name,
		entityType: UMB_TEMPLATE_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		isFolder: false,
		icon: 'icon-newspaper',
	};
};
