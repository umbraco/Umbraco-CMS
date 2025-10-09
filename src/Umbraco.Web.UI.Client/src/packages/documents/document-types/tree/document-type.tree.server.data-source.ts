import { UMB_DOCUMENT_TYPE_ENTITY_TYPE, UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentTypeTreeItemModel } from './types.js';
import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from './folder/index.js';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { DocumentTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Document Type tree that fetches data from the server
 * @class UmbDocumentTypeTreeServerDataSource
 * @augments {UmbTreeServerDataSourceBase}
 */
export class UmbDocumentTypeTreeServerDataSource extends UmbTreeServerDataSourceBase<
	DocumentTypeTreeItemResponseModel,
	UmbDocumentTypeTreeItemModel
> {
	/**
	 * Creates an instance of UmbDocumentTypeTreeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTypeTreeServerDataSource
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
	DocumentTypeService.getTreeDocumentTypeRoot({
		query: { foldersOnly: args.foldersOnly, skip: args.skip, take: args.take },
	});

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parent.unique === null) {
		return getRootItems({
			foldersOnly: args.foldersOnly,
			skip: args.skip,
			take: args.take,
		});
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentTypeService.getTreeDocumentTypeChildren({
			query: { parentId: args.parent.unique, foldersOnly: args.foldersOnly, skip: args.skip, take: args.take },
		});
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	DocumentTypeService.getTreeDocumentTypeAncestors({
		query: { descendantId: args.treeItem.unique },
	});

const mapper = (item: DocumentTypeTreeItemResponseModel): UmbDocumentTypeTreeItemModel => {
	return {
		unique: item.id,
		parent: {
			unique: item.parent ? item.parent.id : null,
			entityType: item.parent ? UMB_DOCUMENT_TYPE_ENTITY_TYPE : UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
		},
		name: item.name,
		entityType: item.isFolder ? UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE : UMB_DOCUMENT_TYPE_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		isFolder: item.isFolder,
		icon: item.icon,
		isElement: item.isElement,
	};
};
