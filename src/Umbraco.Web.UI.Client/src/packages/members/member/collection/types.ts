import type { UmbMemberEntityType } from '../entity.js';

export interface UmbMemberCollectionFilterModel {
	skip?: number;
	take?: number;
}

export interface UmbMemberCollectionModel {
	unique: string;
	name: string;
	entityType: UmbMemberEntityType;
}
