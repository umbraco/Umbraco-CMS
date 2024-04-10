import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentBlueprintTreeItemModel } from './types.js';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import { DocumentBlueprintService } from '@umbraco-cms/backoffice/external/backend-api';
import type { DocumentBlueprintTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

/**
 * A data source for a data type tree that fetches data from the server
 * @export
 * @class UmbDocumentBlueprintTreeServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDocumentBlueprintTreeServerDataSource extends UmbTreeServerDataSourceBase<
	DocumentBlueprintTreeItemResponseModel,
	UmbDocumentBlueprintTreeItemModel
> {
	/**
	 * Creates an instance of UmbDocumentBlueprintTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentBlueprintTreeServerDataSource
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
	DocumentBlueprintService.getTreeDocumentBlueprintRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parentUnique === null) {
		return getRootItems(args);
	} else {
		throw new Error('Not implemented');
		/*
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentBlueprintService.getTreeDocumentBlueprintChildren({
			parentId: args.parentUnique,
		});
		*/
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) => {
	throw new Error('Not implemented');
};

const mapper = (item: DocumentBlueprintTreeItemResponseModel): UmbDocumentBlueprintTreeItemModel => {
	return {
		unique: item.id,
		parentUnique: item.parent?.id || null,
		name: item.name,
		entityType: UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
		isFolder: false,
		hasChildren: item.hasChildren,
	};
};
