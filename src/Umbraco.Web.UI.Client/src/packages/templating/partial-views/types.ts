import type { UmbPartialViewEntityType, UmbPartialViewFolderEntityType } from './entity.js';

export interface UmbPartialViewDetailModel {
	entityType: UmbPartialViewEntityType;
	unique: string;
	parentUnique: string | null;
	path: string;
	name: string;
	content: string;
}

export interface UmbPartialViewItemModel {
	entityType: UmbPartialViewEntityType | UmbPartialViewFolderEntityType;
	unique: string;
	parentUnique: string | null;
	path: string;
	name: string;
}
