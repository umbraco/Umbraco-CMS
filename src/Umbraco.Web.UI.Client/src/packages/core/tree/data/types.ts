import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

interface UmbTargetPagination {
	item: {
		unique: string;
		entityType: string;
	};
	before: number;
	after: number;
}

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
