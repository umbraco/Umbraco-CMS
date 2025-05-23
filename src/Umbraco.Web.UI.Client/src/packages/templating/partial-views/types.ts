import type { UmbPartialViewEntityType, UmbPartialViewFolderEntityType } from './entity.js';

export type * from './tree/types.js';

export interface UmbPartialViewDetailModel {
	entityType: UmbPartialViewEntityType;
	unique: string;
	name: string;
	content: string;
}

export interface UmbPartialViewItemModel {
	entityType: UmbPartialViewEntityType | UmbPartialViewFolderEntityType;
	unique: string;
	parentUnique: string | null;
	name: string;
}
