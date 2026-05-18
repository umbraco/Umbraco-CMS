import type { UmbMemberGroupEntityType } from '../entity.js';
import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export interface UmbMemberGroupCollectionFilterModel {
	skip?: number;
	take?: number;
}

export interface UmbMemberGroupCollectionItemModel extends UmbCollectionItemModel {
	entityType: UmbMemberGroupEntityType;
	name: string;
}
