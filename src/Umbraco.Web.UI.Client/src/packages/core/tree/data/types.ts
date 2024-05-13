import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbTreeRootItemsRequestArgs {
	skip?: number;
	take?: number;
}

export interface UmbTreeChildrenOfRequestArgs {
	parent: UmbEntityModel;
	skip?: number;
	take?: number;
}

export interface UmbTreeAncestorsOfRequestArgs {
	treeItem: {
		unique: string;
		entityType: string;
	};
}
