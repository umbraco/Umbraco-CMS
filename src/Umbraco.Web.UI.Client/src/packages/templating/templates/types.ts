import type { UmbTemplateEntityType } from './entity.js';

export interface UmbTemplateDetailModel {
	entityType: UmbTemplateEntityType;
	unique: string;
	parentUnique: string | null;
	name: string;
	alias: string;
	content: string | null;
	masterTemplateId: string | null;
}
