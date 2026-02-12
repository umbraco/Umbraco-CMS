import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
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
