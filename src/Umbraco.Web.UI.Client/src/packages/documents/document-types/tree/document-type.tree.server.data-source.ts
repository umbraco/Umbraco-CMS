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
import { isOffsetPaginationRequest, isTargetPaginationRequest } from '@umbraco-cms/backoffice/utils';

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

const getRootItems = async (args: UmbTreeRootItemsRequestArgs) => {
	const paging = args.paging;

	if (paging && isTargetPaginationRequest(paging)) {
		if (paging.target.unique === null) {
			throw new Error('Target unique cannot be null when using target pagination');
		}

		// eslint-disable-next-line local-rules/no-direct-api-import
		const { data } = await DocumentTypeService.getTreeDocumentTypeSiblings({
			query: {
				foldersOnly: args.foldersOnly,
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
	const { data } = await DocumentTypeService.getTreeDocumentTypeRoot({
		query: {
			foldersOnly: args.foldersOnly,
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

const getChildrenOf = async (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parent.unique === null) {
		return getRootItems(args);
	}

	const paging = args.paging;

	if (paging && isTargetPaginationRequest(paging)) {
		if (paging.target.unique === null) {
			throw new Error('Target unique cannot be null when using target pagination');
		}

		// eslint-disable-next-line local-rules/no-direct-api-import
		const { data } = await DocumentTypeService.getTreeDocumentTypeSiblings({
			query: {
				foldersOnly: args.foldersOnly,
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
	const { data } = await DocumentTypeService.getTreeDocumentTypeChildren({
		query: {
			parentId: args.parent.unique,
			foldersOnly: args.foldersOnly,
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
