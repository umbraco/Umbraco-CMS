import { UmbStylesheetEntityType } from './entity.js';

export interface UmbStylesheetDetailModel {
	entityType: UmbStylesheetEntityType;
	unique: string;
	parentUnique: string | null;
	path: string;
	name: string;
	content: string;
}

export interface UmbSortableStylesheetRule {
	name: string;
	selector: string;
	styles: string;
	sortOrder: number;
}
