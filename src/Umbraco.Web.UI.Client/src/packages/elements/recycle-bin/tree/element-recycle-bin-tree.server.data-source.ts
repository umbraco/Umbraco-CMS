import { UMB_ELEMENT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../constants.js';
import type { UmbElementRecycleBinTreeItemModel } from '../types.js';
import type { UmbElementTreeItemVariantModel } from '../../tree/types.js';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { ElementRecycleBinItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import type { UmbOffsetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';

/**
 * A data source for the Element Recycle Bin tree that fetches data from the server
 * @class UmbElementRecycleBinTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbElementRecycleBinTreeServerDataSource extends UmbTreeServerDataSourceBase<
	ElementRecycleBinItemResponseModel,
	UmbElementRecycleBinTreeItemModel
> {
	/**
	 * Creates an instance of UmbElementRecycleBinTreeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementRecycleBinTreeServerDataSource
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
	const { data, ...rest } = await ElementService.getRecycleBinElementRoot({
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
		const { data, ...rest } = await ElementService.getRecycleBinElementChildren({
			query: { parentId: args.parent.unique, skip, take },
		});
		return { data: { ...data, totalBefore: 0, totalAfter: Math.max(data.total - data.items.length, 0) }, ...rest };
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	ElementService.getTreeElementAncestors({
		query: { descendantId: args.treeItem.unique },
	});

// TODO: Review the commented out properties. [LK:2026-01-06]
const mapper = (item: ElementRecycleBinItemResponseModel): UmbElementRecycleBinTreeItemModel => {
	return {
		unique: item.id,
		parent: {
			unique: item.parent ? item.parent.id : null,
			entityType: item.parent
				? item.isFolder
					? UMB_ELEMENT_FOLDER_ENTITY_TYPE
					: UMB_ELEMENT_ENTITY_TYPE
				: UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE,
		},
		entityType: item.isFolder ? UMB_ELEMENT_FOLDER_ENTITY_TYPE : UMB_ELEMENT_ENTITY_TYPE,
		icon: item.isFolder ? 'icon-folder' : (item.documentType?.icon ?? 'icon-document'),
		isTrashed: true,
		hasChildren: item.hasChildren,
		//isProtected: false,
		documentType: {
			unique: item.documentType?.id ?? '',
			icon: item.isFolder ? 'icon-folder' : (item.documentType?.icon ?? 'icon-document'),
			collection: null,
		},
		variants: item.variants.map((variant): UmbElementTreeItemVariantModel => {
			return {
				name: variant.name,
				culture: variant.culture || null,
				segment: null, // TODO: add segment to the backend API?
				state: variant.state,
				//flags: variant.flags ?? [],
			};
		}),
		// TODO: this is not correct. We need to get it from the variants. This is a temp solution. [LK]
		name: item.isFolder ? item.name : item.variants[0]?.name,
		isFolder: item.isFolder,
		createDate: item.createDate,
		// TODO: Recycle bin items should have flags, but the API does not return any at the moment. [NL]
		flags: (item as any).flags ?? [],
	};
};
