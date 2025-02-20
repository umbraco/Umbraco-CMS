import type { UmbMemberTypeEntityType } from '../../entity.js';

export interface UmbMemberTypeItemModel {
	entityType: UmbMemberTypeEntityType;
	unique: string;
	name: string;
	icon: string;
}
