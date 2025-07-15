import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbTreeRootItemsRequestArgs {
	foldersOnly?: boolean;
	skip?: number;
	take?: number;
}

export interface UmbTreeChildrenOfRequestArgs {
	parent: UmbEntityModel;
	foldersOnly?: boolean;
	skip?: number;
	take?: number;
}

export interface UmbTreeAncestorsOfRequestArgs {
	treeItem: {
		unique: string;
		entityType: string;
	};
}
