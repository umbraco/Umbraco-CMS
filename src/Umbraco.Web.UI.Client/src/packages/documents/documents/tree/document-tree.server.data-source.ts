import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../entity.js';
import type {
	UmbDocumentTreeChildrenOfRequestArgs,
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootItemsRequestArgs,
} from './types.js';
import type { UmbTreeAncestorsOfRequestArgs } from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Document tree that fetches data from the server
 * @class UmbDocumentTreeServerDataSource
 * @augments {UmbTreeServerDataSourceBase}
 */
export class UmbDocumentTreeServerDataSource extends UmbTreeServerDataSourceBase<
	DocumentTreeItemResponseModel,
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootItemsRequestArgs,
	UmbDocumentTreeChildrenOfRequestArgs
> {
	/**
	 * Creates an instance of UmbDocumentTreeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTreeServerDataSource
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

const getRootItems = (args: UmbDocumentTreeRootItemsRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	DocumentService.getTreeDocumentRoot({
		dataTypeId: args.dataType?.unique,
		skip: args.skip,
		take: args.take,
	});

const getChildrenOf = (args: UmbDocumentTreeChildrenOfRequestArgs) => {
	if (args.parent.unique === null) {
		return getRootItems(args);
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentService.getTreeDocumentChildren({
			parentId: args.parent.unique,
			dataTypeId: args.dataType?.unique,
			skip: args.skip,
			take: args.take,
		});
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	DocumentService.getTreeDocumentAncestors({
		descendantId: args.treeItem.unique,
	});

const mapper = (item: DocumentTreeItemResponseModel): UmbDocumentTreeItemModel => {
	return {
		ancestors: item.ancestors.map((ancestor) => {
			return {
				unique: ancestor.id,
				entityType: UMB_DOCUMENT_ENTITY_TYPE,
			};
		}),
		unique: item.id,
		parent: {
			unique: item.parent ? item.parent.id : null,
			entityType: item.parent ? UMB_DOCUMENT_ENTITY_TYPE : UMB_DOCUMENT_ROOT_ENTITY_TYPE,
		},
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
		noAccess: item.noAccess,
		isTrashed: item.isTrashed,
		hasChildren: item.hasChildren,
		isProtected: item.isProtected,
		documentType: {
			unique: item.documentType.id,
			icon: item.documentType.icon,
			collection: item.documentType.collection ? { unique: item.documentType.collection.id } : null,
		},
		variants: item.variants.map((variant) => {
			return {
				name: variant.name,
				culture: variant.culture || null,
				segment: null, // TODO: add segment to the backend API?
				state: variant.state,
			};
		}),
		name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
		isFolder: false,
		createDate: item.createDate,
	};
};
