import { UmbStylesheetEntityType } from './entity.js';

export interface UmbStylesheetDetailModel {
	entityType: UmbStylesheetEntityType;
	unique: string;
	parentUnique: string | null;
	path: string;
	name: string;
	content: string;
}
