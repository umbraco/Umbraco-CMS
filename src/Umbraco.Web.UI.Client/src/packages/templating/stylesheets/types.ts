import { UmbStylesheetEntityType } from './entity.js';

export interface UmbStylesheetDetailModel {
	entityType: UmbStylesheetEntityType;
	unique: string;
	parentUnique: string | null;
	path: string;
	name: string;
	content: string;
}

export interface UmbStylesheetRule {
	name: string;
	selector: string;
	styles: string;
}
