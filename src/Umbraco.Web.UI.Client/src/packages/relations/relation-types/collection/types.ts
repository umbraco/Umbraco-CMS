import type { UmbRelationTypeDetailModel } from '../types.js';
import type { UmbRelationTypeEntityType } from '../entity.js';
import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export interface UmbRelationTypeCollectionItemModel extends UmbRelationTypeDetailModel, UmbCollectionItemModel {
	entityType: UmbRelationTypeEntityType;
}

export interface UmbRelationTypeCollectionFilterModel {
	skip?: number;
	take?: number;
}
