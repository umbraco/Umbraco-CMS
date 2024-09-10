import type { UmbMemberEntityType } from '../entity.js';
import type { UmbMemberKindType } from '../utils/index.js';

export interface UmbMemberCollectionFilterModel {
	skip?: number;
	take?: number;
	memberTypeId?: string;
	filter?: string;
}

export interface UmbMemberCollectionModel {
	unique: string;
	entityType: UmbMemberEntityType;
	variants: Array<any>;
	kind: UmbMemberKindType;
}
