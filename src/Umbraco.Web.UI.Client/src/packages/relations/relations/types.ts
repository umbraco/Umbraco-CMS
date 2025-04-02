import type { UmbRelationEntityType } from './entity.js';
export type * from './global-components/types.js';
export type * from './reference/types.js';

export interface UmbRelationDetailModel {
	unique: string;
	entityType: UmbRelationEntityType;
	relationType: {
		unique: string;
	};
	parent: {
		unique: string;
		name: string;
	};
	child: {
		unique: string;
		name: string;
	};
	createDate: string;
	comment: string | null;
}
