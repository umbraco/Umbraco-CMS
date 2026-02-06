import type { UmbExtensionEntityType } from '../entity.js';
import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export interface UmbExtensionCollectionFilterModel extends UmbCollectionFilterModel {
	type?: string;
}

export interface UmbExtensionCollectionItemModel extends UmbCollectionItemModel {
	entityType: UmbExtensionEntityType;
	type: string;
	alias: string;
	name: string;
	weight?: number;
}
