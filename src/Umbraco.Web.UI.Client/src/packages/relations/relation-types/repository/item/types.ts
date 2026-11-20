import type { UmbRelationTypeEntityType } from '../../entity.js';

export interface UmbRelationTypeItemModel {
	entityType: UmbRelationTypeEntityType;
	unique: string;
	name: string;
}
