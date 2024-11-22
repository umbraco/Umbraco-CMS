import type { UmbMemberGroupEntityType } from '../entity.js';

export interface UmbMemberGroupCollectionFilterModel {
	skip?: number;
	take?: number;
}

export interface UmbMemberGroupCollectionModel {
	entityType: UmbMemberGroupEntityType;
	name: string;
	unique: string;
}
