import type { UmbRelationTypeDetailModel } from '../types.js';
import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export interface UmbRelationTypeCollectionItemModel extends UmbRelationTypeDetailModel, UmbCollectionItemModel {}

export interface UmbRelationTypeCollectionFilterModel {
	skip?: number;
	take?: number;
}
