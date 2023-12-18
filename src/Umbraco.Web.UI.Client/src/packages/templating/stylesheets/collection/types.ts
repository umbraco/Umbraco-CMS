export interface UmbStylesheetCollectionFilterModel {
	/**
	 * The number of items to take.
	 */
	take?: number;

	/**
	 * The number of items to skip.
	 */
	skip?: number;
}

export interface UmbStylesheetCollectionItemModel {
	name: string;
	path: string;
}
