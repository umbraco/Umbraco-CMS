import type { UmbTemplateEntityType } from './entity.js';

export interface UmbTemplateDetailModel {
	entityType: UmbTemplateEntityType;
	unique: string;
	name: string;
	alias: string;
	content: string | null;
	layoutTemplate: { unique: string } | null;
	/**
	 * @deprecated Use layoutTemplate instead. Scheduled for removal in Umbraco 20.
	 */
	masterTemplate?: { unique: string } | null;
}
