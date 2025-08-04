export interface UmbTargetPagination {
	item: {
		unique: string;
		entityType: string;
	};
	before: number;
	after: number;
}
