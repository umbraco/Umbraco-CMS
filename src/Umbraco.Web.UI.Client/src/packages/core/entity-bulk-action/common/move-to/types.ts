export interface UmbBulkMoveToRequestArgs {
	uniques: Array<string>;
	destination: {
		unique: string | null;
	};
}
