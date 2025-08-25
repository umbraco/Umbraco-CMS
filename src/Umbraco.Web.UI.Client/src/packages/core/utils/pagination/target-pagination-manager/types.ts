export interface UmbTargetPaginationModel {
	item: {
		unique: string;
		entityType: string;
	};
	takeBefore: number;
	takeAfter: number;
}
