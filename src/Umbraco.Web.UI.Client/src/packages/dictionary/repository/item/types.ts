import type { UmbDictionaryEntityType } from '../../entity.js';

export interface UmbDictionaryItemModel {
	entityType: UmbDictionaryEntityType;
	unique: string;
	name: string;
}
