export interface UmbTreeRootItemsRequestArgs {
	skip?: number;
	take?: number;
}

export interface UmbTreeChildrenOfRequestArgs {
	parent: {
		unique: string | null;
		entityType: string;
	};
	skip?: number;
	take?: number;
}

export interface UmbTreeAncestorsOfRequestArgs {
	treeItem: {
		unique: string;
		entityType: string;
	};
}
