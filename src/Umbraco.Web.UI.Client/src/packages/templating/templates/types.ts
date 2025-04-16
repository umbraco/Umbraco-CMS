import type { UmbTemplateEntityType } from './entity.js';

export interface UmbTemplateDetailModel {
	entityType: UmbTemplateEntityType;
	unique: string;
	name: string;
	alias: string;
	content: string | null;
	masterTemplate: { unique: string } | null;
}
