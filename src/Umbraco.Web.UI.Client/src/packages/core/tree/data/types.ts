export interface UmbTreeRootItemsRequestArgs {
	skip: number;
	take: number;
}

export interface UmbTreeChildrenOfRequestArgs {
	parentUnique: string | null;
	skip: number;
	take: number;
}
