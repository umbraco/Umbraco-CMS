import { UmbStylesheetEntityType, UmbStylesheetFolderEntityType } from './entity.js';

export interface UmbStylesheetDetailModel {
	entityType: UmbStylesheetEntityType;
	unique: string;
	parentUnique: string | null;
	path: string;
	name: string;
	content: string;
}

export interface UmbStylesheetItemModel {
	entityType: UmbStylesheetEntityType | UmbStylesheetFolderEntityType;
	unique: string;
	parentUnique: string | null;
	name: string;
	isFolder: boolean;
}

export interface UmbStylesheetRule {
	name: string;
	selector: string;
	styles: string;
}
