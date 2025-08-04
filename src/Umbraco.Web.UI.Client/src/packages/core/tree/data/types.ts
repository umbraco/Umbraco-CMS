import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbTargetPagination } from '@umbraco-cms/backoffice/utils';

export interface UmbTreeRootItemsRequestArgs {
	foldersOnly?: boolean;
	skip?: number;
	take?: number;
	target?: UmbTargetPagination;
}

export interface UmbTreeChildrenOfRequestArgs {
	parent: UmbEntityModel;
	foldersOnly?: boolean;
	skip?: number;
	take?: number;
	target?: UmbTargetPagination;
}

export interface UmbTreeAncestorsOfRequestArgs {
	treeItem: {
		unique: string;
		entityType: string;
	};
}
