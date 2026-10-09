import type { UmbRelationTypeEntityType } from '../entity.js';
import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export interface UmbRelationTypeCollectionItemModel extends UmbCollectionItemModel {
	entityType: UmbRelationTypeEntityType;
	alias: string;
	name: string;
	child: {
		objectType: {
			unique: string;
			name: string;
		};
	} | null;
	isBidirectional: boolean;
	isDependency: boolean;
	parent: {
		objectType: {
			unique: string;
			name: string;
		};
	} | null;
}

export interface UmbRelationTypeCollectionFilterModel {
	skip?: number;
	take?: number;
}
