import type { UmbLanguageEntityType } from './entity.js';

export interface UmbLanguageDetailModel {
	entityType: UmbLanguageEntityType;
	unique: string;
	name: string;
	isDefault: boolean;
	isMandatory: boolean;
	fallbackIsoCode: string | null;
}
