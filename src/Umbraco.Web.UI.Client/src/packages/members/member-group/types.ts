import type { UmbMemberGroupEntityType } from './entity.js';

export interface UmbMemberGroupDetailModel {
	entityType: UmbMemberGroupEntityType;
	unique: string;
	name: string;
}

export type { UmbMemberGroupEntityType, UmbMemberGroupRootEntityType } from './entity.js';

export type * from './repository/types.js';
