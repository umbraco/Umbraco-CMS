import type { UmbEntityUnique } from './entity-unique/types.js';

export interface UmbEntityModel {
	unique: UmbEntityUnique;
	entityType: string;
	documentTypeUnique?: string;
}

export interface UmbNamedEntityModel extends UmbEntityModel {
	name: string;
}

export type * from './entity-type/types.js';
export type * from './entity-unique/types.js';
