import type { UmbTemplateEntityType } from '../../entity.js';

export interface UmbTemplateItemModel {
	entityType: UmbTemplateEntityType;
	unique: string;
	name: string;
	alias: string;
}
