import type { UmbPartialViewEntityType } from './entity.js';

export interface UmbPartialViewDetailModel {
	entityType: UmbPartialViewEntityType;
	unique: string;
	path: string;
	name: string;
	content: string;
}
