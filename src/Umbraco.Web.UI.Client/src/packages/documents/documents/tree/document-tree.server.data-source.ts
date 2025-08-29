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
import { isOffsetPaginationRequest, isTargetPaginationRequest } from '@umbraco-cms/backoffice/utils';

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

const getRootItems = async (args: UmbDocumentTreeRootItemsRequestArgs) => {
	const paging = args.paging;

	if (paging && isTargetPaginationRequest(paging)) {
		// eslint-disable-next-line local-rules/no-direct-api-import
		const { data } = await DocumentService.getTreeDocumentSiblings({
			query: {
				dataTypeId: args.dataType?.unique,
				target: paging.target.unique,
				before: paging.takeBefore,
				after: paging.takeAfter,
			},
		});

		return {
			data: {
				items: data.items,
				total: data.totalBefore + data.items.length + data.totalAfter,
				totalBefore: data.totalBefore,
				totalAfter: data.totalAfter,
			},
		};
	}

	const skip = paging && isOffsetPaginationRequest(paging) ? paging.skip : args.skip ? args.skip : 0;
	const take = paging && isOffsetPaginationRequest(paging) ? paging.take : args.take ? args.take : 50;

	// eslint-disable-next-line local-rules/no-direct-api-import
	const { data } = await DocumentService.getTreeDocumentRoot({
		query: {
			dataTypeId: args.dataType?.unique,
			skip,
			take,
		},
	});

	return {
		data: {
			items: data.items,
			total: data.total,
			totalBefore: 0,
			totalAfter: data.total - data.items.length,
		},
	};
};

const getChildrenOf = async (args: UmbDocumentTreeChildrenOfRequestArgs) => {
	if (args.parent.unique === null) {
		return getRootItems(args);
	}

	const paging = args.paging;

	if (paging && isTargetPaginationRequest(paging)) {
		// eslint-disable-next-line local-rules/no-direct-api-import
		const { data } = await DocumentService.getTreeDocumentSiblings({
			query: {
				dataTypeId: args.dataType?.unique,
				target: paging.target.unique,
				before: paging.takeBefore,
				after: paging.takeAfter,
			},
		});

		return {
			data: {
				items: data.items,
				total: data.totalBefore + data.items.length + data.totalAfter,
				totalBefore: data.totalBefore,
				totalAfter: data.totalAfter,
			},
		};
	}

	// Including args.skip + args.take for backwards compatibility
	const skip = paging && isOffsetPaginationRequest(paging) ? paging.skip : args.skip ? args.skip : 0;
	const take = paging && isOffsetPaginationRequest(paging) ? paging.take : args.take ? args.take : 50;

	// eslint-disable-next-line local-rules/no-direct-api-import
	const { data } = await DocumentService.getTreeDocumentChildren({
		query: {
			parentId: args.parent.unique,
			dataTypeId: args.dataType?.unique,
			skip,
			take,
		},
	});

	return {
		data: {
			items: data.items,
			total: data.total,
			totalBefore: 0,
			totalAfter: data.total - data.items.length,
		},
	};
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	DocumentService.getTreeDocumentAncestors({
		query: {
			descendantId: args.treeItem.unique,
		},
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
