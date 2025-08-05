export interface UmbTargetPaginationModel {
	item: {
		unique: string;
		entityType: string;
	};
	before: number;
	after: number;
}
