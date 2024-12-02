import type { UmbMemberGroupEntityType } from './entity.js';

export interface UmbMemberGroupDetailModel {
	entityType: UmbMemberGroupEntityType;
	unique: string;
	name: string;
}
