import type { UmbSegmentEntityType } from './entity.js';

export type { UmbSegmentEntityType } from './entity.js';
export type * from './repository/types.js';

export interface UmbSegmentDetailModel {
	entityType: UmbSegmentEntityType;
	unique: string;
	alias: string;
}
