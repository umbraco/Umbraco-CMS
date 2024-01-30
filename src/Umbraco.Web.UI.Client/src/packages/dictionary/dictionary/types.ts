import type { UmbDictionaryEntityType } from './entity.js';

export interface UmbDictionaryDetailModel {
	entityType: UmbDictionaryEntityType;
	unique: string;
	parentUnique: string | null;
	name: string;
	translations: Array<UmbDictionaryTranslationModel>;
}

export interface UmbDictionaryTranslationModel {
	isoCode: string;
	translation: string;
}
