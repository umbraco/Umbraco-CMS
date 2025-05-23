import {
	UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
} from '../entity.js';
import type { UmbDocumentBlueprintTreeItemModel } from './types.js';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import { DocumentBlueprintService } from '@umbraco-cms/backoffice/external/backend-api';
import type { DocumentBlueprintTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbTreeChildrenOfRequestArgs, UmbTreeRootItemsRequestArgs } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for a data type tree that fetches data from the server
 * @class UmbDocumentBlueprintTreeServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDocumentBlueprintTreeServerDataSource extends UmbTreeServerDataSourceBase<
	DocumentBlueprintTreeItemResponseModel,
	UmbDocumentBlueprintTreeItemModel
> {
	/**
	 * Creates an instance of UmbDocumentBlueprintTreeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
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
	DocumentBlueprintService.getTreeDocumentBlueprintRoot({
		query: { foldersOnly: args.foldersOnly, skip: args.skip, take: args.take },
	});

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parent.unique === null) {
		return getRootItems(args);
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentBlueprintService.getTreeDocumentBlueprintChildren({
			query: { parentId: args.parent.unique, foldersOnly: args.foldersOnly },
		});
	}
};

const getAncestorsOf = () => {
	throw new Error('Not implemented');
	/** TODO: Implement when endpoint becomes available... */
};

const mapper = (item: DocumentBlueprintTreeItemResponseModel): UmbDocumentBlueprintTreeItemModel => {
	return {
		unique: item.id,
		parent: {
			unique: item.parent ? item.parent.id : null,
			entityType: item.parent ? UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE : UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
		},
		name: (item as any).variants?.[0].name ?? item.name,
		entityType: item.isFolder ? UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE : UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
		isFolder: item.isFolder,
		hasChildren: item.hasChildren,
		icon: item.isFolder ? 'icon-folder' : 'icon-blueprint',
	};
};
