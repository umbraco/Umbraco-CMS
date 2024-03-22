import type { UmbRelationTypeEntityType } from './entity.js';

export interface UmbRelationTypeDetailModel {
	alias: string;
	child: {
		objectType: {
			unique: string;
			name: string;
		};
	} | null;
	entityType: UmbRelationTypeEntityType;
	isBidirectional: boolean;
	isDependency: boolean;
	name: string;
	parent: {
		objectType: {
			unique: string;
			name: string;
		};
	} | null;
	unique: string;
}
