import type { UmbLanguageDetailModel, UmbLanguageEntityType } from '../types.js';
import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export interface UmbLanguageCollectionFilterModel {
	skip?: number;
	take?: number;
}

export interface UmbLanguageCollectionItemModel extends UmbCollectionItemModel, UmbLanguageDetailModel {
	entityType: UmbLanguageEntityType;
	name: string;
}
