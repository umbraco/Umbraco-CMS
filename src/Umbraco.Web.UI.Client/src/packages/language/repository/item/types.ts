import type { UmbLanguageEntityType } from '../../entity.js';

export interface UmbLanguageItemModel {
	entityType: UmbLanguageEntityType;
	unique: string;
	name: string;
}
