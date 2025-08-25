import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbTargetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';

export interface UmbTreeRootItemsRequestArgs {
	foldersOnly?: boolean;
	skip?: number;
	take?: number;
	target?: UmbTargetPaginationRequestModel;
}

export interface UmbTreeChildrenOfRequestArgs {
	parent: UmbEntityModel;
	foldersOnly?: boolean;
	skip?: number;
	take?: number;
	target?: UmbTargetPaginationRequestModel;
}

export interface UmbTreeAncestorsOfRequestArgs {
	treeItem: {
		unique: string;
		entityType: string;
	};
}
