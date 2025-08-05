import type { UmbPagedModel } from '@umbraco-cms/backoffice/repository';
export interface UmbTargetPaginationModel {
	item: {
		unique: string;
		entityType: string;
	};
	before: number;
	after: number;
}

export interface UmbTargetPagedModel<T> extends UmbPagedModel<T> {
	// TODO: v18: make mandatory
	totalBefore?: number;
	totalAfter?: number;
}
