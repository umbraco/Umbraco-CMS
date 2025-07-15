export interface UmbBulkDuplicateToRequestArgs {
	uniques: Array<string>;
	destination: {
		unique: string | null;
	};
}
