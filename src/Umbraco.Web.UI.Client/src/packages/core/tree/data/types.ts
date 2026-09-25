import type { UmbTreeItemModel } from '../types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbItemDataResolver } from '@umbraco-cms/backoffice/entity-item';
import type { UmbOffsetPaginationRequestModel, UmbTargetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';

export interface UmbTreeRootItemsRequestArgs {
	foldersOnly?: boolean;
	paging?: UmbOffsetPaginationRequestModel | UmbTargetPaginationRequestModel;
}

export interface UmbTreeChildrenOfRequestArgs {
	parent: UmbEntityModel;
	foldersOnly?: boolean;
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
