import type { UmbDictionaryEntityType } from '../entity.js';

export interface UmbDictionaryCollectionFilterModel {
	skip?: number;
	take?: number;
}

export interface UmbDictionaryCollectionModel {
	entityType: UmbDictionaryEntityType;
	name: string;
	parentUnique: string | null;
	translatedIsoCodes: Array<string>;
	unique: string;
}
