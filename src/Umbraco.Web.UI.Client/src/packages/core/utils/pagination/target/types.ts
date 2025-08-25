export interface UmbTargetPaginationRequestModel {
	target: {
		unique: string;
		entityType: string;
	};
	takeBefore: number;
	takeAfter: number;
}
