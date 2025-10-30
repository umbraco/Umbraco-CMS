import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbTargetPaginationRequestModel {
	target: UmbEntityModel;
	takeBefore: number;
	takeAfter: number;
}
