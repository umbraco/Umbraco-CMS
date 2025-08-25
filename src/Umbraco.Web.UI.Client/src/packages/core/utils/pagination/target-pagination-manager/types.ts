export interface UmbTargetPaginationModel {
	target: {
		unique: string;
		entityType: string;
	};
	takeBefore: number;
	takeAfter: number;
}
