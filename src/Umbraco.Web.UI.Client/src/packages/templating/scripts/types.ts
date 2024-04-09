import type { UmbScriptEntityType, UmbScriptFolderEntityType } from './entity.js';

export interface UmbScriptDetailModel {
	entityType: UmbScriptEntityType;
	unique: string;
	name: string;
	content: string;
}

export interface UmbScriptItemModel {
	entityType: UmbScriptEntityType | UmbScriptFolderEntityType;
	unique: string;
	parentUnique: string | null;
	name: string;
	isFolder: boolean;
}
