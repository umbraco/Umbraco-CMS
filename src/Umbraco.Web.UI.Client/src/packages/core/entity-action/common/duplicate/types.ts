export interface UmbDuplicateToRequestArgs {
	unique: string;
	destination: {
		unique: string | null;
	};
	relateToOriginal: boolean;
	includeDescendants: boolean;
}
