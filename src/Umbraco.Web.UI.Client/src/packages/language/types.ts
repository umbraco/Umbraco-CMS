import type { UmbLanguageEntityType } from './entity.js';

export type { UmbLanguageEntityType, UmbLanguageRootEntityType } from './entity.js';
export type * from './conditions/language-user-permission/types.js';
export type * from './conditions/multiple-app-languages/types.js';
export type * from './repository/types.js';

export interface UmbLanguageDetailModel {
	entityType: UmbLanguageEntityType;
	unique: string;
	name: string;
	isDefault: boolean;
	isMandatory: boolean;
	fallbackIsoCode: string | null;
}
