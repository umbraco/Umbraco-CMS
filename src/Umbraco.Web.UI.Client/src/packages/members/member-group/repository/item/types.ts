import type { UmbMemberGroupEntityType } from '../../entity.js';

export interface UmbMemberGroupItemModel {
	entityType: UmbMemberGroupEntityType;
	unique: string;
	name: string;
}
