import type { UmbStylesheetEntityType, UmbStylesheetFolderEntityType } from './entity.js';

export interface UmbStylesheetDetailModel {
	entityType: UmbStylesheetEntityType;
	unique: string;
	name: string;
	content: string;
}

export interface UmbStylesheetItemModel {
	entityType: UmbStylesheetEntityType | UmbStylesheetFolderEntityType;
	unique: string;
	name: string;
	isFolder: boolean;
}
