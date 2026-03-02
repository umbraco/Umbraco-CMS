import type { UmbLanguageEntityType } from '../entity.js';
import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export interface UmbLanguageCollectionFilterModel {
	skip?: number;
	take?: number;
}

export interface UmbLanguageCollectionItemModel extends UmbCollectionItemModel {
	entityType: UmbLanguageEntityType;
	name: string;
	isDefault: boolean;
	isMandatory: boolean;
	fallbackIsoCode: string | null;
}
