import type { UmbScriptEntityType, UmbScriptFolderEntityType } from './entity.js';

export interface UmbScriptDetailModel {
	entityType: UmbScriptEntityType;
	unique: string;
	parentUnique: string | null;
	path: string;
	name: string;
	content: string;
}

export interface UmbScriptItemModel {
	entityType: UmbScriptEntityType | UmbScriptFolderEntityType;
	unique: string;
	parentUnique: string | null;
	path: string;
	name: string;
	isFolder: boolean;
}
