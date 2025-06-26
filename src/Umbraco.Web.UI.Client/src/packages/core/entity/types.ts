export type UmbEntityUnique = string | null;

export interface UmbEntityModel {
	unique: UmbEntityUnique;
	entityType: string;
}

export interface UmbNamedEntityModel extends UmbEntityModel {
	name: string;
}

export type * from './entity-type/types.js';
export type * from './entity-unique/types.js';
