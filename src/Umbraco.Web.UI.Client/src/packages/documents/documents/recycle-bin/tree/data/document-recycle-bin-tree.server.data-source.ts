import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../../constants.js';
import type { UmbDocumentRecycleBinTreeItemModel } from '../types.js';
import type { DocumentRecycleBinItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { UmbOffsetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';

/**
 * A data source for the Document Recycle Bin tree that fetches data from the server
 * @class UmbDocumentRecycleBinTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbDocumentRecycleBinTreeServerDataSource extends UmbTreeServerDataSourceBase<
	DocumentRecycleBinItemResponseModel,
	UmbDocumentRecycleBinTreeItemModel
> {
	/**
	 * Creates an instance of UmbDocumentRecycleBinTreeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentRecycleBinTreeServerDataSource
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
	const { skip = 0, take = 100 } = (args.paging ?? {}) as UmbOffsetPaginationRequestModel;
	// eslint-disable-next-line local-rules/no-direct-api-import
	const { data, ...rest } = await DocumentService.getRecycleBinDocumentRoot({
		query: { skip, take },
	});
	return { data: { ...data, totalBefore: 0, totalAfter: Math.max(data.total - data.items.length, 0) }, ...rest };
};

const getChildrenOf = async (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parent.unique === null) {
		return getRootItems(args);
	} else {
		const { skip = 0, take = 100 } = (args.paging ?? {}) as UmbOffsetPaginationRequestModel;
		// eslint-disable-next-line local-rules/no-direct-api-import
		const { data, ...rest } = await DocumentService.getRecycleBinDocumentChildren({
			query: { parentId: args.parent.unique, skip, take },
		});
		return { data: { ...data, totalBefore: 0, totalAfter: Math.max(data.total - data.items.length, 0) }, ...rest };
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	DocumentService.getTreeDocumentAncestors({
		query: { descendantId: args.treeItem.unique },
	});

const mapper = (item: DocumentRecycleBinItemResponseModel): UmbDocumentRecycleBinTreeItemModel => {
	return {
		unique: item.id,
		parent: {
			unique: item.parent ? item.parent.id : null,
			entityType: item.parent ? UMB_DOCUMENT_ENTITY_TYPE : UMB_DOCUMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE,
		},
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
		noAccess: false,
		isTrashed: true,
		hasChildren: item.hasChildren,
		isProtected: false,
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
				flags: variant.flags ?? [],
			};
		}),
		name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
		isFolder: false,
		createDate: item.createDate,
		// TODO: Recycle bin items should have flags, but the API does not return any at the moment. [NL]
		flags: (item as any).flags ?? [],
	};
};
