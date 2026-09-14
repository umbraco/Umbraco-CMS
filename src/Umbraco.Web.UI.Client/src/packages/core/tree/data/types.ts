import type { UmbTreeItemModel } from '../types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbItemDataResolver } from '@umbraco-cms/backoffice/entity-item';
import type { UmbOffsetPaginationRequestModel, UmbTargetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';

export interface UmbTreeRootItemsRequestArgs {
	foldersOnly?: boolean;
	/**
	 * @deprecated - Deprecated from 16.3. Use paging instead. To be removed in v18.
	 * @type {number} - The number of items to skip
	 * @memberof UmbTreeRootItemsRequestArgs
	 */
	skip?: number;
	/**
	 * @deprecated - Deprecated from 16.3. Use paging instead. To be removed in v18.
	 * @type {number} - The number of items to take
	 * @memberof UmbTreeRootItemsRequestArgs
	 */
	take?: number;
	paging?: UmbOffsetPaginationRequestModel | UmbTargetPaginationRequestModel;
}

export interface UmbTreeChildrenOfRequestArgs {
	parent: UmbEntityModel;
	foldersOnly?: boolean;
	/**
	 * @deprecated - Deprecated from 16.3. Use paging instead. To be removed in v18.
	 * @type {number} - The number of items to skip
	 * @memberof UmbTreeChildrenOfRequestArgs
	 */
	skip?: number;
	/**
	 * @deprecated - Deprecated from 16.3. Use paging instead. To be removed in v18.
	 * @type {number} - The number of items to take
	 * @memberof UmbTreeChildrenOfRequestArgs
	 */
	take?: number;
	paging?: UmbOffsetPaginationRequestModel | UmbTargetPaginationRequestModel;
}

export interface UmbTreeAncestorsOfRequestArgs {
	treeItem: {
		unique: string;
		entityType: string;
	};
}

export interface UmbCreateTreeItemDataResolverArgs {
	entityType: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbTreeItemDataResolver<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
> extends UmbItemDataResolver<TreeItemType> {}
