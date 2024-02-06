import type { UmbPartialViewEntityType } from './entity.js';

export interface UmbPartialViewDetailModel {
	entityType: UmbPartialViewEntityType;
	unique: string;
	parentUnique: string | null;
	path: string;
	name: string;
	content: string;
}
