import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbTargetPaginationModel } from '@umbraco-cms/backoffice/utils';

export interface UmbTreeRootItemsRequestArgs {
	foldersOnly?: boolean;
	skip?: number;
	take?: number;
	target?: UmbTargetPaginationModel;
}

export interface UmbTreeChildrenOfRequestArgs {
	parent: UmbEntityModel;
	foldersOnly?: boolean;
	skip?: number;
	take?: number;
	target?: UmbTargetPaginationModel;
}

export interface UmbTreeAncestorsOfRequestArgs {
	treeItem: {
		unique: string;
		entityType: string;
	};
}
